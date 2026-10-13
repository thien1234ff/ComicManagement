using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Dialogs
{
    public class DeadlineEditDialog : Form
    {
        private readonly DeadlineService _deadlineService = new();
        private readonly MemberService _memberService = new();
        private readonly ComicService _comicService = new();
        private readonly CongDoanDeadline? _existingDeadline;
        private readonly int? _preselectedChapterId;

        private ComboBox cboChapter = null!;
        private ComboBox cboStage = null!;
        private ComboBox cboMember = null!;
        private DateTimePicker dtpStart = null!;
        private DateTimePicker dtpDeadline = null!;
        private ComboBox cboStatus = null!;
        private TextBox txtLink = null!;
        private TextBox txtNote = null!;
        private Label lblError = null!;

        public DeadlineEditDialog(CongDoanDeadline? deadline = null, int? preselectedChapterId = null)
        {
            _existingDeadline = deadline;
            _preselectedChapterId = preselectedChapterId;
            InitializeComponent();
            LoadDropdownData();
            PopulateData();
        }

        private void InitializeComponent()
        {
            Text = _existingDeadline == null ? "Giao Việc & Lập Deadline Mới" : $"Chỉnh Sửa Deadline: {_existingDeadline.TenCongDoan}";
            Size = new Size(580, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(520, 600),
                Location = new Point(22, 18),
                BorderRadius = 16,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = _existingDeadline == null ? "GIAO VIỆC & THIẾT LẬP DEADLINE" : "CẬP NHẬT CÔNG ĐOẠN BIÊN TẬP",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = StudioColors.PrimaryPurple,
                Location = new Point(20, 16),
                Size = new Size(480, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            int currentY = 52;

            // 1. Chọn Chapter
            AddLabel("Bộ truyện & Chapter trực thuộc *", card, 28, currentY);
            var pnlChap = new RoundedPanel
            {
                Location = new Point(28, currentY + 22),
                Size = new Size(460, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboChapter = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlChap
            };
            currentY += 66;

            // 2. Tên công đoạn & Thành viên phụ trách
            AddLabel("Tên công đoạn Pipeline *", card, 28, currentY, 220);
            AddLabel("Thành viên phụ trách *", card, 268, currentY, 220);

            var pnlStage = new RoundedPanel
            {
                Location = new Point(28, currentY + 22),
                Size = new Size(220, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboStage = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlStage
            };
            cboStage.Items.AddRange(new object[]
            {
                PipelineStages.Translate,
                PipelineStages.CleanRedraw,
                PipelineStages.Typeset,
                PipelineStages.QualityCheck
            });
            cboStage.SelectedIndex = 0;

            var pnlMem = new RoundedPanel
            {
                Location = new Point(268, currentY + 22),
                Size = new Size(220, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboMember = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlMem
            };
            currentY += 66;

            // 3. Ngày bắt đầu & Hạn chót
            AddLabel("Ngày bắt đầu", card, 28, currentY, 220);
            AddLabel("Hạn chót (Deadline) *", card, 268, currentY, 220);

            dtpStart = new DateTimePicker
            {
                Location = new Point(28, currentY + 24),
                Size = new Size(220, 26),
                CustomFormat = "dd/MM/yyyy HH:mm",
                Format = DateTimePickerFormat.Custom,
                Font = new Font("Segoe UI", 9.5f),
                Parent = card
            };

            dtpDeadline = new DateTimePicker
            {
                Location = new Point(268, currentY + 24),
                Size = new Size(220, 26),
                CustomFormat = "dd/MM/yyyy HH:mm",
                Format = DateTimePickerFormat.Custom,
                Font = new Font("Segoe UI", 9.5f),
                Value = DateTime.Now.AddDays(2),
                Parent = card
            };
            currentY += 62;

            // 4. Trạng thái & Link nộp sản phẩm
            AddLabel("Trạng thái tiến độ *", card, 28, currentY, 220);
            AddLabel("Link sản phẩm (Drive/OneDrive)", card, 268, currentY, 220);

            var pnlStatus = new RoundedPanel
            {
                Location = new Point(28, currentY + 22),
                Size = new Size(220, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboStatus = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlStatus
            };
            cboStatus.Items.AddRange(new object[]
            {
                TaskStatuses.NotStarted,
                TaskStatuses.InProgress,
                TaskStatuses.PendingApproval,
                TaskStatuses.Completed
            });
            cboStatus.SelectedIndex = 1;

            txtLink = AddInput(card, 268, currentY + 22, 220);
            currentY += 66;

            // 5. Ghi chú yêu cầu
            AddLabel("Ghi chú / Yêu cầu đặc biệt cho công đoạn", card, 28, currentY);
            txtNote = AddInput(card, 28, currentY + 22, 460);
            currentY += 70;

            lblError = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.DangerRed,
                Location = new Point(28, currentY),
                Size = new Size(460, 20),
                Parent = card
            };
            currentY += 24;

            // Buttons
            var btnSave = new RoundedButton
            {
                Text = _existingDeadline == null ? "LẬP DEADLINE" : "CẬP NHẬT",
                Location = new Point(28, currentY),
                Size = new Size(225, 40),
                BorderRadius = 8,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = card
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new RoundedButton
            {
                Text = "Hủy bỏ",
                Location = new Point(263, currentY),
                Size = new Size(225, 40),
                BorderRadius = 8,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnCancel.Click += (s, e) => Close();
        }

        private void AddLabel(string text, Control parent, int x, int y, int width = 460)
        {
            var lbl = new Label
            {
                Text = text,
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(x, y),
                Size = new Size(width, 20),
                Parent = parent
            };
        }

        private TextBox AddInput(Control parent, int x, int y, int width = 460)
        {
            var pnl = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(width, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 8, 10, 8),
                Parent = parent
            };
            var txt = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                Parent = pnl
            };
            return txt;
        }

        private void LoadDropdownData()
        {
            // Load Chapters
            var comics = _comicService.GetAllComics();
            foreach (var c in comics)
            {
                foreach (var ch in c.Chapters)
                {
                    cboChapter.Items.Add(new ComboBoxItem<int>(ch.Id, $"{c.TenTruyen} - Chap {ch.SoChapter}"));
                }
            }

            if (cboChapter.Items.Count > 0)
            {
                if (_preselectedChapterId.HasValue)
                {
                    for (int i = 0; i < cboChapter.Items.Count; i++)
                    {
                        if (cboChapter.Items[i] is ComboBoxItem<int> item && item.Value == _preselectedChapterId.Value)
                        {
                            cboChapter.SelectedIndex = i;
                            break;
                        }
                    }
                }
                if (cboChapter.SelectedIndex < 0) cboChapter.SelectedIndex = 0;
            }

            // Load Members
            cboMember.Items.Add(new ComboBoxItem<int?>(null, "-- Chưa phân công --"));
            var members = _memberService.GetAllMembers();
            foreach (var m in members)
            {
                cboMember.Items.Add(new ComboBoxItem<int?>(m.Id, $"{m.HoTen} ({m.VaiTroChuyenMon})"));
            }
            cboMember.SelectedIndex = 0;
        }

        private void PopulateData()
        {
            if (_existingDeadline == null) return;

            for (int i = 0; i < cboChapter.Items.Count; i++)
            {
                if (cboChapter.Items[i] is ComboBoxItem<int> item && item.Value == _existingDeadline.ChapterId)
                {
                    cboChapter.SelectedIndex = i;
                    break;
                }
            }

            cboStage.SelectedItem = _existingDeadline.TenCongDoan;

            for (int i = 0; i < cboMember.Items.Count; i++)
            {
                if (cboMember.Items[i] is ComboBoxItem<int?> item && item.Value == _existingDeadline.ThanhVienId)
                {
                    cboMember.SelectedIndex = i;
                    break;
                }
            }

            dtpStart.Value = _existingDeadline.NgayBatDau;
            dtpDeadline.Value = _existingDeadline.HanChot;
            cboStatus.SelectedItem = _existingDeadline.TrangThai;
            txtLink.Text = _existingDeadline.LinkNopSanPham ?? "";
            txtNote.Text = _existingDeadline.GhiChu ?? "";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            if (cboChapter.SelectedItem is not ComboBoxItem<int> selectedChap)
            {
                lblError.Text = "Vui lòng chọn Chapter!";
                return;
            }

            int chapterId = selectedChap.Value;
            int? memberId = (cboMember.SelectedItem as ComboBoxItem<int?>)?.Value;
            string stage = cboStage.SelectedItem?.ToString() ?? PipelineStages.Translate;
            string status = cboStatus.SelectedItem?.ToString() ?? TaskStatuses.InProgress;

            if (dtpDeadline.Value < dtpStart.Value)
            {
                lblError.Text = "Hạn chót phải sau ngày bắt đầu!";
                return;
            }

            if (_existingDeadline == null)
            {
                var newDeadline = new CongDoanDeadline
                {
                    ChapterId = chapterId,
                    TenCongDoan = stage,
                    ThanhVienId = memberId,
                    NgayBatDau = dtpStart.Value,
                    HanChot = dtpDeadline.Value,
                    TrangThai = status,
                    LinkNopSanPham = txtLink.Text.Trim(),
                    GhiChu = txtNote.Text.Trim()
                };

                var res = _deadlineService.AddDeadline(newDeadline);
                if (!res.Success)
                {
                    lblError.Text = res.Message;
                    return;
                }
            }
            else
            {
                _existingDeadline.ChapterId = chapterId;
                _existingDeadline.TenCongDoan = stage;
                _existingDeadline.ThanhVienId = memberId;
                _existingDeadline.NgayBatDau = dtpStart.Value;
                _existingDeadline.HanChot = dtpDeadline.Value;
                _existingDeadline.TrangThai = status;
                _existingDeadline.LinkNopSanPham = txtLink.Text.Trim();
                _existingDeadline.GhiChu = txtNote.Text.Trim();

                var res = _deadlineService.UpdateDeadline(_existingDeadline);
                if (!res.Success)
                {
                    lblError.Text = res.Message;
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class ComboBoxItem<T>
    {
        public T Value { get; set; }
        public string Text { get; set; }

        public ComboBoxItem(T val, string text)
        {
            Value = val;
            Text = text;
        }

        public override string ToString() => Text;
    }
}
