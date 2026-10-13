using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Views
{
    public class DeadlineKanbanView : UserControl
    {
        private readonly DeadlineService _deadlineService = new();
        private readonly ComicService _comicService = new();
        private readonly MemberService _memberService = new();

        private ComboBox cboComicFilter = null!;
        private ComboBox cboMemberFilter = null!;
        private ComboBox cboUrgencyFilter = null!;
        private TextBox txtSearch = null!;

        // 6 Pipeline Columns
        private FlowLayoutPanel pnlColScan = null!;
        private FlowLayoutPanel pnlColTrans = null!;
        private FlowLayoutPanel pnlColClean = null!;
        private FlowLayoutPanel pnlColTypeset = null!;
        private FlowLayoutPanel pnlColQC = null!;
        private FlowLayoutPanel pnlColDone = null!;

        private Label lblCountScan = null!;
        private Label lblCountTrans = null!;
        private Label lblCountClean = null!;
        private Label lblCountTypeset = null!;
        private Label lblCountQC = null!;
        private Label lblCountDone = null!;

        public DeadlineKanbanView()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = StudioColors.BackgroundDark;
            InitializeComponent();
            LoadDropdowns();
            LoadKanbanBoards();
        }

        private void InitializeComponent()
        {
            // 1. Top Filters Toolbar
            var pnlToolbar = new RoundedPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(14, 10, 14, 10)
            };

            // Search
            var pnlSearch = new RoundedPanel
            {
                Location = new Point(14, 11),
                Size = new Size(210, 38),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 8, 10, 8),
                Parent = pnlToolbar
            };
            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                PlaceholderText = "Tìm truyện, task...",
                Parent = pnlSearch
            };
            txtSearch.TextChanged += (s, e) => LoadKanbanBoards();

            // Comic Filter
            var lblComic = new Label { Text = "Truyện:", Font = StudioColors.SmallFont, ForeColor = StudioColors.TextMuted, Location = new Point(235, 20), AutoSize = true, Parent = pnlToolbar };
            var pnlComic = new RoundedPanel { Location = new Point(280, 11), Size = new Size(160, 38), BorderRadius = 6, CustomBackgroundColor = StudioColors.CardDark, BorderColor = StudioColors.BorderColor, Padding = new Padding(6), Parent = pnlToolbar };
            cboComicFilter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = StudioColors.CardDark, ForeColor = StudioColors.TextWhite, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Parent = pnlComic };
            cboComicFilter.SelectedIndexChanged += (s, e) => LoadKanbanBoards();

            // Member Filter
            var lblMember = new Label { Text = "Thành viên:", Font = StudioColors.SmallFont, ForeColor = StudioColors.TextMuted, Location = new Point(450, 20), AutoSize = true, Parent = pnlToolbar };
            var pnlMember = new RoundedPanel { Location = new Point(520, 11), Size = new Size(150, 38), BorderRadius = 6, CustomBackgroundColor = StudioColors.CardDark, BorderColor = StudioColors.BorderColor, Padding = new Padding(6), Parent = pnlToolbar };
            cboMemberFilter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = StudioColors.CardDark, ForeColor = StudioColors.TextWhite, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Parent = pnlMember };
            cboMemberFilter.SelectedIndexChanged += (s, e) => LoadKanbanBoards();

            // Urgency Filter
            var lblUrgency = new Label { Text = "Hạn chót:", Font = StudioColors.SmallFont, ForeColor = StudioColors.TextMuted, Location = new Point(680, 20), AutoSize = true, Parent = pnlToolbar };
            var pnlUrgency = new RoundedPanel { Location = new Point(735, 11), Size = new Size(150, 38), BorderRadius = 6, CustomBackgroundColor = StudioColors.CardDark, BorderColor = StudioColors.BorderColor, Padding = new Padding(6), Parent = pnlToolbar };
            cboUrgencyFilter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = StudioColors.CardDark, ForeColor = StudioColors.TextWhite, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Parent = pnlUrgency };
            cboUrgencyFilter.Items.AddRange(new object[] { "Tất cả", "An toàn (> 2 ngày)", "Sắp hết hạn (<= 48h)", "Quá hạn (Overdue)", "Đã hoàn thành" });
            cboUrgencyFilter.SelectedIndex = 0;
            cboUrgencyFilter.SelectedIndexChanged += (s, e) => LoadKanbanBoards();

            // Add Deadline Button
            var btnAddDeadline = new RoundedButton
            {
                Text = "+ Giao Việc Mới",
                Location = new Point(895, 11),
                Size = new Size(150, 38),
                BorderRadius = 6,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = pnlToolbar
            };
            btnAddDeadline.Click += (s, e) =>
            {
                var dlg = new Dialogs.DeadlineEditDialog();
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadKanbanBoards();
            };

            // 2. Kanban Board Columns Container
            var pnlKanbanScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            var pnlColumnsContainer = new FlowLayoutPanel
            {
                Location = new Point(0, 10),
                Size = new Size(1850, 680),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Parent = pnlKanbanScroll
            };

            Controls.Add(pnlToolbar);
            Controls.Add(pnlKanbanScroll);
            pnlKanbanScroll.BringToFront(); // Ngăn chặn toolbar đè lấp hàng trên của Kanban!

            int colWidth = 285;
            pnlColScan = CreateKanbanColumn("1. RAW / SCAN", StudioColors.TextMuted, pnlColumnsContainer, colWidth, out lblCountScan);
            pnlColTrans = CreateKanbanColumn("2. DỊCH (TRANS)", StudioColors.CyanNeon, pnlColumnsContainer, colWidth, out lblCountTrans);
            pnlColClean = CreateKanbanColumn("3. CLEAN & REDRAW", StudioColors.PrimaryPurple, pnlColumnsContainer, colWidth, out lblCountClean);
            pnlColTypeset = CreateKanbanColumn("4. TYPESET", StudioColors.SecondaryPink, pnlColumnsContainer, colWidth, out lblCountTypeset);
            pnlColQC = CreateKanbanColumn("5. DUYỆT (QC)", StudioColors.WarningAmber, pnlColumnsContainer, colWidth, out lblCountQC);
            pnlColDone = CreateKanbanColumn("6. HOÀN TẤT", StudioColors.SafeGreen, pnlColumnsContainer, colWidth, out lblCountDone);
        }

        private FlowLayoutPanel CreateKanbanColumn(string title, Color accentColor, Control parent, int width, out Label countLabel)
        {
            var colPanel = new RoundedPanel
            {
                Size = new Size(width, 660),
                BorderRadius = 10,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Margin = new Padding(0, 0, 12, 0),
                Padding = new Padding(10),
                Parent = parent
            };

            var pnlHead = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Parent = colPanel
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(4, 6),
                AutoSize = true,
                Parent = pnlHead
            };

            countLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                BackColor = Color.FromArgb(50, 50, 75),
                Location = new Point(width - 48, 5),
                Size = new Size(28, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = pnlHead
            };

            var pnlCardsList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0),
                BackColor = Color.Transparent,
                Parent = colPanel
            };

            return pnlCardsList;
        }

        private void LoadDropdowns()
        {
            cboComicFilter.Items.Clear();
            cboComicFilter.Items.Add(new Dialogs.ComboBoxItem<int?>(null, "Tất cả truyện"));
            var comics = _comicService.GetAllComics();
            foreach (var c in comics)
            {
                cboComicFilter.Items.Add(new Dialogs.ComboBoxItem<int?>(c.Id, c.TenTruyen));
            }
            cboComicFilter.SelectedIndex = 0;

            cboMemberFilter.Items.Clear();
            cboMemberFilter.Items.Add(new Dialogs.ComboBoxItem<int?>(null, "Tất cả thành viên"));
            var members = _memberService.GetAllMembers();
            foreach (var m in members)
            {
                cboMemberFilter.Items.Add(new Dialogs.ComboBoxItem<int?>(m.Id, m.HoTen));
            }
            cboMemberFilter.SelectedIndex = 0;
        }

        public void LoadKanbanBoards()
        {
            pnlColScan.Controls.Clear();
            pnlColTrans.Controls.Clear();
            pnlColClean.Controls.Clear();
            pnlColTypeset.Controls.Clear();
            pnlColQC.Controls.Clear();
            pnlColDone.Controls.Clear();

            int? comicId = (cboComicFilter.SelectedItem as Dialogs.ComboBoxItem<int?>)?.Value;
            int? memberId = (cboMemberFilter.SelectedItem as Dialogs.ComboBoxItem<int?>)?.Value;
            string? kw = txtSearch.Text.Trim();

            DeadlineUrgency? urgency = null;
            if (cboUrgencyFilter.SelectedIndex == 1) urgency = DeadlineUrgency.Safe;
            else if (cboUrgencyFilter.SelectedIndex == 2) urgency = DeadlineUrgency.Warning;
            else if (cboUrgencyFilter.SelectedIndex == 3) urgency = DeadlineUrgency.Overdue;
            else if (cboUrgencyFilter.SelectedIndex == 4) urgency = DeadlineUrgency.Completed;

            var deadlines = _deadlineService.GetDeadlines(comicId, memberId, null, null, urgency, kw);

            int cntScan = 0, cntTrans = 0, cntClean = 0, cntTypeset = 0, cntQC = 0, cntDone = 0;

            foreach (var d in deadlines)
            {
                var card = new DeadlineItemCard
                {
                    Deadline = d,
                    Width = 250,
                    Height = 130
                };

                card.QuickActionClicked += (s, task) =>
                {
                    var dlg = new Dialogs.QuickProgressDialog(task);
                    if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadKanbanBoards();
                };

                card.EditClicked += (s, task) =>
                {
                    var dlg = new Dialogs.DeadlineEditDialog(task);
                    if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadKanbanBoards();
                };

                if (d.TrangThai == TaskStatuses.Completed)
                {
                    pnlColDone.Controls.Add(card);
                    cntDone++;
                }
                else if (d.TenCongDoan.Contains("Dịch") || d.TenCongDoan.Contains("Trans"))
                {
                    pnlColTrans.Controls.Add(card);
                    cntTrans++;
                }
                else if (d.TenCongDoan.Contains("Clean") || d.TenCongDoan.Contains("Xóa") || d.TenCongDoan.Contains("Redraw"))
                {
                    pnlColClean.Controls.Add(card);
                    cntClean++;
                }
                else if (d.TenCongDoan.Contains("Typeset") || d.TenCongDoan.Contains("Điền"))
                {
                    pnlColTypeset.Controls.Add(card);
                    cntTypeset++;
                }
                else if (d.TenCongDoan.Contains("QC") || d.TenCongDoan.Contains("Duyệt") || d.TenCongDoan.Contains("Kiểm tra"))
                {
                    pnlColQC.Controls.Add(card);
                    cntQC++;
                }
                else
                {
                    pnlColScan.Controls.Add(card);
                    cntScan++;
                }
            }

            lblCountScan.Text = cntScan.ToString();
            lblCountTrans.Text = cntTrans.ToString();
            lblCountClean.Text = cntClean.ToString();
            lblCountTypeset.Text = cntTypeset.ToString();
            lblCountQC.Text = cntQC.ToString();
            lblCountDone.Text = cntDone.ToString();
        }
    }
}
