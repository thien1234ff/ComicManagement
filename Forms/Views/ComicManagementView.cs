using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Views
{
    public class ComicManagementView : UserControl
    {
        private readonly ComicService _comicService = new();

        private TextBox txtSearch = null!;
        private ComboBox cboGenreFilter = null!;
        private ComboBox cboStatusFilter = null!;
        private FlowLayoutPanel pnlCards = null!;
        private DataGridView dgvComics = null!;
        private DataGridView dgvChapters = null!;
        private RoundedPanel pnlDetail = null!;
        private Label lblDetailTitle = null!;
        private Label lblDetailStats = null!;
        private Truyen? _selectedComic;
        private bool _isCardView = true;
        private RoundedButton btnToggleView = null!;

        public ComicManagementView()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = StudioColors.BackgroundDark;
            InitializeComponent();
            LoadComics();
        }

        private void InitializeComponent()
        {
            // 1. Top Filter Toolbar
            var pnlToolbar = new RoundedPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(12, 8, 12, 8)
            };

            // Search
            var pnlSearch = new RoundedPanel
            {
                Location = new Point(12, 9),
                Size = new Size(250, 36),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 7, 10, 7),
                Parent = pnlToolbar
            };
            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                PlaceholderText = "Tìm truyện, tác giả...",
                Parent = pnlSearch
            };
            txtSearch.TextChanged += (s, e) => LoadComics();

            // Genre Filter
            var lblGenre = new Label
            {
                Text = "Loại:",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(272, 18),
                AutoSize = true,
                Parent = pnlToolbar
            };

            var pnlGenre = new RoundedPanel
            {
                Location = new Point(310, 9),
                Size = new Size(115, 36),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6, 4, 6, 4),
                Parent = pnlToolbar
            };
            cboGenreFilter = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Parent = pnlGenre
            };
            cboGenreFilter.Items.AddRange(new object[] { "Tất cả", "Manga", "Manhwa", "Comic" });
            cboGenreFilter.SelectedIndex = 0;
            cboGenreFilter.SelectedIndexChanged += (s, e) => LoadComics();

            // Status Filter
            var lblStatus = new Label
            {
                Text = "Trạng thái:",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(435, 18),
                AutoSize = true,
                Parent = pnlToolbar
            };

            var pnlStatus = new RoundedPanel
            {
                Location = new Point(500, 9),
                Size = new Size(125, 36),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6, 4, 6, 4),
                Parent = pnlToolbar
            };
            cboStatusFilter = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Parent = pnlStatus
            };
            cboStatusFilter.Items.AddRange(new object[] { "Tất cả", ComicStatuses.InProgress, ComicStatuses.OnHold, ComicStatuses.Completed });
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += (s, e) => LoadComics();

            // Toggle Card/Table View
            btnToggleView = new RoundedButton
            {
                Text = "Dạng Lưới Bìa",
                Location = new Point(638, 9),
                Size = new Size(115, 36),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.CyanNeon,
                Parent = pnlToolbar
            };
            btnToggleView.Click += BtnToggleView_Click;

            // Add Comic Button
            var btnAddComic = new RoundedButton
            {
                Text = "+ Thêm Bộ Truyện",
                Location = new Point(765, 9),
                Size = new Size(145, 36),
                BorderRadius = 6,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = pnlToolbar
            };
            btnAddComic.Click += (s, e) =>
            {
                var dlg = new Dialogs.ComicEditDialog();
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadComics();
            };

            // 2. Content Body
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = Color.Transparent
            };

            // Right Detail Inspector
            pnlDetail = new RoundedPanel
            {
                Dock = DockStyle.Right,
                Width = 480,
                BorderRadius = 10,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(14, 12, 14, 12)
            };

            // Left Cards/Table
            pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = StudioColors.BackgroundDark,
                Padding = new Padding(6, 6, 6, 6)
            };

            dgvComics = CreateStyledDataGrid();
            dgvComics.Dock = DockStyle.Fill;
            dgvComics.Visible = false;
            dgvComics.SelectionChanged += DgvComics_SelectionChanged;

            // Thứ tự Add vào pnlBody: Detail trước, rồi đến Cards/Table
            pnlBody.Controls.Add(pnlDetail);
            pnlBody.Controls.Add(dgvComics);
            pnlBody.Controls.Add(pnlCards);
            pnlCards.BringToFront();
            dgvComics.BringToFront();

            // Thứ tự Add vào this: Toolbar trước, rồi Body Dock Fill
            Controls.Add(pnlToolbar);
            Controls.Add(pnlBody);
            pnlBody.BringToFront(); // Đảm bảo Body nằm dưới Toolbar, không bao giờ bị đè!

            SetupDetailPanel();
        }

        private void SetupDetailPanel()
        {
            // Header cố định tọa độ, thoáng đãng
            var pnlDetailHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 126,
                BackColor = Color.Transparent,
                Parent = pnlDetail
            };

            // 1. Tên truyện
            lblDetailTitle = new Label
            {
                Text = "Chi Tiết Bộ Truyện",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(0, 4),
                Size = new Size(450, 26),
                Parent = pnlDetailHeader
            };

            // 2. Thống kê phụ
            lblDetailStats = new Label
            {
                Text = "Chọn một bộ truyện để xem danh sách Chapter & công đoạn.",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(0, 32),
                Size = new Size(450, 20),
                Parent = pnlDetailHeader
            };

            // 3. Hàng nút thao tác bộ truyện (Độ rộng chuẩn 120-140px, chữ hiển thị 100%)
            var btnAddChapter = new RoundedButton
            {
                Text = "+ Thêm Chapter",
                Location = new Point(0, 58),
                Size = new Size(140, 32),
                BorderRadius = 6,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = pnlDetailHeader
            };
            btnAddChapter.Click += (s, e) =>
            {
                if (_selectedComic == null) return;
                var dlg = new Dialogs.ChapterEditDialog(_selectedComic.Id);
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    LoadComics();
                    SelectComic(_selectedComic.Id);
                }
            };

            var btnEditComic = new RoundedButton
            {
                Text = "Sửa Thông Tin",
                Location = new Point(142, 58),
                Size = new Size(125, 32),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.CyanNeon,
                Parent = pnlDetailHeader
            };
            btnEditComic.Click += (s, e) =>
            {
                if (_selectedComic == null) return;
                var dlg = new Dialogs.ComicEditDialog(_selectedComic);
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    LoadComics();
                    SelectComic(_selectedComic.Id);
                }
            };

            var btnDeleteComic = new RoundedButton
            {
                Text = "Xóa Truyện",
                Location = new Point(274, 58),
                Size = new Size(115, 32),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(60, 30, 40),
                HoverColor = StudioColors.DangerRed,
                Parent = pnlDetailHeader
            };
            btnDeleteComic.Click += (s, e) =>
            {
                if (_selectedComic == null) return;
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa bộ truyện '{_selectedComic.TenTruyen}' cùng toàn bộ Chapters và Deadlines liên quan?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _comicService.DeleteComic(_selectedComic.Id);
                    _selectedComic = null;
                    LoadComics();
                }
            };

            // 4. Tiêu đề danh sách Chapter
            var lblChapterTitle = new Label
            {
                Text = "DANH SÁCH CHAPTER",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                Location = new Point(0, 100),
                Size = new Size(400, 22),
                Parent = pnlDetailHeader
            };

            // 5. Chapter table (Dock Fill)
            dgvChapters = CreateStyledDataGrid();
            dgvChapters.Dock = DockStyle.Fill;
            dgvChapters.ReadOnly = false;
            dgvChapters.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvChapters.DataError += (s, e) => { e.Cancel = false; };
            dgvChapters.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvChapters.IsCurrentCellDirty)
                {
                    dgvChapters.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            dgvChapters.CellValueChanged += DgvChapters_CellValueChanged;
            pnlDetail.Controls.Add(dgvChapters);
            dgvChapters.BringToFront(); // Luôn nằm ngay dưới pnlDetailHeader, chiếm trọn phần dưới

            var ctx = new ContextMenuStrip();
            var mnuEditChap = ctx.Items.Add("Chỉnh sửa Chapter");
            mnuEditChap.Click += (s, e) =>
            {
                if (dgvChapters.SelectedRows.Count > 0)
                {
                    int chapId = (int)dgvChapters.SelectedRows[0].Cells["Id"].Value;
                    var chap = _selectedComic?.Chapters.FirstOrDefault(c => c.Id == chapId);
                    if (chap != null && _selectedComic != null)
                    {
                        var dlg = new Dialogs.ChapterEditDialog(_selectedComic.Id, chap);
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                        {
                            LoadComics();
                            SelectComic(_selectedComic.Id);
                        }
                    }
                }
            };

            var mnuDelChap = ctx.Items.Add("Xóa Chapter này");
            mnuDelChap.Click += (s, e) =>
            {
                if (dgvChapters.SelectedRows.Count > 0)
                {
                    int chapId = (int)dgvChapters.SelectedRows[0].Cells["Id"].Value;
                    if (MessageBox.Show("Bạn có chắc muốn xóa Chapter này và toàn bộ task biên tập của nó?",
                        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _comicService.DeleteChapter(chapId);
                        LoadComics();
                        if (_selectedComic != null) SelectComic(_selectedComic.Id);
                    }
                }
            };
            dgvChapters.ContextMenuStrip = ctx;
        }

        private void DgvChapters_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvChapters.Columns[e.ColumnIndex].DataPropertyName == "TrangThai")
            {
                int chapId = (int)dgvChapters.Rows[e.RowIndex].Cells["Id"].Value;
                string? newStatus = dgvChapters.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (!string.IsNullOrEmpty(newStatus))
                {
                    _comicService.UpdateChapterStatus(chapId, newStatus);
                    if (_selectedComic != null)
                    {
                        var refreshed = _comicService.GetComicById(_selectedComic.Id);
                        if (refreshed != null)
                        {
                            _selectedComic = refreshed;
                            lblDetailStats.Text = $"Tác giả: {_selectedComic.TacGia} • {_selectedComic.LoaiTruyen} ({_selectedComic.TheLoai})\nTiến độ: {_selectedComic.PublishedChapters}/{_selectedComic.TotalChapters} chapters ({_selectedComic.CompletionPercentage}%)";
                        }
                    }
                }
            }
        }

        private DataGridView CreateStyledDataGrid()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                GridColor = StudioColors.BorderColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                RowTemplate = { Height = 34 }
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = StudioColors.SidebarDark;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = StudioColors.CyanNeon;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersHeight = 38;

            dgv.DefaultCellStyle.BackColor = StudioColors.CardDark;
            dgv.DefaultCellStyle.ForeColor = StudioColors.TextWhite;
            dgv.DefaultCellStyle.SelectionBackColor = StudioColors.PrimaryPurple;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = StudioColors.RegularFont;

            return dgv;
        }

        private void BtnToggleView_Click(object? sender, EventArgs e)
        {
            _isCardView = !_isCardView;
            pnlCards.Visible = _isCardView;
            dgvComics.Visible = !_isCardView;
            btnToggleView.Text = _isCardView ? "Dạng Lưới Bìa" : "Dạng Bảng";
        }

        public void LoadComics()
        {
            string? kw = txtSearch.Text.Trim();
            string? genre = cboGenreFilter.SelectedItem?.ToString();
            string? status = cboStatusFilter.SelectedItem?.ToString();

            var list = _comicService.GetAllComics(kw, genre, status);

            pnlCards.Controls.Clear();
            foreach (var comic in list)
            {
                var card = new CoverCardControl
                {
                    Comic = comic
                };
                card.ComicClicked += (s, c) => SelectComic(c.Id);
                pnlCards.Controls.Add(card);
            }

            dgvComics.DataSource = null;
            dgvComics.Columns.Clear();
            dgvComics.AutoGenerateColumns = false;

            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50, Name = "Id" });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenTruyen", HeaderText = "Tên Truyện", Width = 220 });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TacGia", HeaderText = "Tác Giả", Width = 140 });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TheLoai", HeaderText = "Thể Loại", Width = 160 });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LoaiTruyen", HeaderText = "Loại", Width = 80 });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TrangThai", HeaderText = "Trạng Thái", Width = 120 });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalChapters", HeaderText = "Tổng Chap", Width = 90 });
            dgvComics.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CompletionPercentage", HeaderText = "% Hoàn Tất", Width = 90 });

            dgvComics.DataSource = list;

            if (list.Count > 0)
            {
                SelectComic(list[0].Id);
            }
            else
            {
                _selectedComic = null;
                lblDetailTitle.Text = "Không tìm thấy bộ truyện nào";
                lblDetailStats.Text = "";
                dgvChapters.DataSource = null;
            }
        }

        private void DgvComics_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvComics.SelectedRows.Count > 0)
            {
                int id = (int)dgvComics.SelectedRows[0].Cells["Id"].Value;
                SelectComic(id);
            }
        }

        private void SelectComic(int comicId)
        {
            _selectedComic = _comicService.GetComicById(comicId);
            if (_selectedComic == null) return;

            lblDetailTitle.Text = _selectedComic.TenTruyen;
            lblDetailStats.Text = $"Tác giả: {_selectedComic.TacGia} • {_selectedComic.LoaiTruyen} ({_selectedComic.TheLoai})\nTiến độ: {_selectedComic.PublishedChapters}/{_selectedComic.TotalChapters} chapters ({_selectedComic.CompletionPercentage}%)";

            var chapters = _comicService.GetChaptersByComicId(comicId);
            dgvChapters.DataSource = null;
            dgvChapters.Columns.Clear();
            dgvChapters.AutoGenerateColumns = false;

            var colId = new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 40, Name = "Id", ReadOnly = true };
            var colChap = new DataGridViewTextBoxColumn { DataPropertyName = "SoChapter", HeaderText = "Chap #", Width = 70, ReadOnly = true };
            var colTitle = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TieuDe",
                HeaderText = "Tiêu Đề",
                MinimumWidth = 110,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };

            var colStatus = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng Thái",
                Width = 145,
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            colStatus.Items.AddRange("Chưa bắt đầu", "Đang biên tập", "Đã xuất bản");
            colStatus.DefaultCellStyle.BackColor = StudioColors.CardDark;
            colStatus.DefaultCellStyle.ForeColor = StudioColors.CyanNeon;
            colStatus.DefaultCellStyle.SelectionBackColor = StudioColors.PrimaryPurple;
            colStatus.DefaultCellStyle.SelectionForeColor = Color.White;

            var colProgress = new DataGridViewTextBoxColumn { DataPropertyName = "ProgressPercent", HeaderText = "Tiến Độ %", Width = 80, ReadOnly = true };

            dgvChapters.Columns.AddRange(colId, colChap, colTitle, colStatus, colProgress);
            dgvChapters.DataSource = chapters;
        }
    }
}
