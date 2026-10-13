using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Views
{
    public class MemberManagementView : UserControl
    {
        private readonly MemberService _memberService = new();

        private TextBox txtSearch = null!;
        private ComboBox cboRoleFilter = null!;
        private ComboBox cboStatusFilter = null!;
        private Label lblCount = null!;
        private DataGridView dgvMembers = null!;
        private List<MemberTableRowModel> _currentRows = new();

        public MemberManagementView()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = StudioColors.BackgroundDark;
            InitializeComponent();
            SetupGridColumns();
            LoadMembers();
        }

        private void InitializeComponent()
        {
            // 1. Toolbar
            var pnlToolbar = new RoundedPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(14, 10, 14, 10)
            };

            // Search Live
            var pnlSearch = new RoundedPanel
            {
                Location = new Point(14, 11),
                Size = new Size(220, 38),
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
                PlaceholderText = "Tìm tên, username, email...",
                Parent = pnlSearch
            };
            txtSearch.TextChanged += (s, e) => LoadMembers();

            // Role filter
            var lblRole = new Label 
            { 
                Text = "Vai trò:", 
                Font = StudioColors.SmallFont, 
                ForeColor = StudioColors.TextMuted, 
                Location = new Point(246, 20), 
                AutoSize = true, 
                Parent = pnlToolbar 
            };
            var pnlRole = new RoundedPanel 
            { 
                Location = new Point(298, 11), 
                Size = new Size(130, 38), 
                BorderRadius = 6, 
                CustomBackgroundColor = StudioColors.CardDark, 
                BorderColor = StudioColors.BorderColor, 
                Padding = new Padding(6, 4, 6, 4), 
                Parent = pnlToolbar 
            };
            cboRoleFilter = new ComboBox 
            { 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                BackColor = StudioColors.CardDark, 
                ForeColor = StudioColors.TextWhite, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 9f), 
                Parent = pnlRole 
            };
            cboRoleFilter.Items.Add("Tất cả");
            cboRoleFilter.Items.AddRange(StudioRoles.AllRoles);
            cboRoleFilter.SelectedIndex = 0;
            cboRoleFilter.SelectedIndexChanged += (s, e) => LoadMembers();

            // Status filter
            var lblStatus = new Label 
            { 
                Text = "Trạng thái:", 
                Font = StudioColors.SmallFont, 
                ForeColor = StudioColors.TextMuted, 
                Location = new Point(440, 20), 
                AutoSize = true, 
                Parent = pnlToolbar 
            };
            var pnlStatus = new RoundedPanel 
            { 
                Location = new Point(510, 11), 
                Size = new Size(115, 38), 
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
            cboStatusFilter.Items.Add("Tất cả");
            cboStatusFilter.Items.AddRange(MemberStatuses.All);
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += (s, e) => LoadMembers();

            // Counter Label
            lblCount = new Label
            {
                Text = "Tổng: 0 thành viên",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(638, 20),
                AutoSize = true,
                Parent = pnlToolbar
            };

            // Add Member Button
            var btnAdd = new RoundedButton
            {
                Text = "+ Thêm Thành Viên Mới",
                Location = new Point(780, 11),
                Size = new Size(185, 38),
                BorderRadius = 6,
                NormalColor = StudioColors.SecondaryPink,
                HoverColor = Color.FromArgb(244, 114, 182),
                Parent = pnlToolbar
            };
            btnAdd.Click += (s, e) =>
            {
                var dlg = new Dialogs.MemberEditDialog();
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadMembers();
            };

            // 2. Table Container
            var pnlGridWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 10, 14, 14),
                BackColor = StudioColors.BackgroundDark
            };

            dgvMembers = CreateStyledDataGrid();
            dgvMembers.Dock = DockStyle.Fill;
            pnlGridWrapper.Controls.Add(dgvMembers);

            // Context Menu
            var ctx = new ContextMenuStrip();
            var mnuEdit = new ToolStripMenuItem("Sửa thông tin thành viên");
            mnuEdit.Click += (s, e) =>
            {
                if (dgvMembers.SelectedRows.Count > 0)
                {
                    int idx = dgvMembers.SelectedRows[0].Index;
                    if (idx >= 0 && idx < _currentRows.Count)
                    {
                        var dlg = new Dialogs.MemberEditDialog(_currentRows[idx].Dto.Member);
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadMembers();
                    }
                }
            };
            var mnuDelete = new ToolStripMenuItem("Xóa thành viên");
            mnuDelete.Click += (s, e) =>
            {
                if (dgvMembers.SelectedRows.Count > 0)
                {
                    int idx = dgvMembers.SelectedRows[0].Index;
                    if (idx >= 0 && idx < _currentRows.Count)
                    {
                        DeleteMemberPrompt(_currentRows[idx].Dto.Member);
                    }
                }
            };
            var mnuRefresh = new ToolStripMenuItem("Làm mới danh sách");
            mnuRefresh.Click += (s, e) => LoadMembers();
            ctx.Items.AddRange(new ToolStripItem[] { mnuEdit, mnuDelete, new ToolStripSeparator(), mnuRefresh });
            dgvMembers.ContextMenuStrip = ctx;

            // Events
            dgvMembers.CellContentClick += DgvMembers_CellContentClick;
            dgvMembers.CellDoubleClick += DgvMembers_CellDoubleClick;
            dgvMembers.CellFormatting += DgvMembers_CellFormatting;
            dgvMembers.CellPainting += DgvMembers_CellPainting;
            dgvMembers.CellValueChanged += DgvMembers_CellValueChanged;
            dgvMembers.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvMembers.IsCurrentCellDirty)
                    dgvMembers.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            Controls.Add(pnlToolbar);
            Controls.Add(pnlGridWrapper);
            pnlGridWrapper.BringToFront(); // Đảm bảo bảng không bao giờ bị toolbar đè lên!
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
                AllowUserToResizeRows = false,
                ReadOnly = false,
                EditMode = DataGridViewEditMode.EditOnEnter,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                RowTemplate = { Height = 38 }
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = StudioColors.SidebarDark;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = StudioColors.CyanNeon;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.BackColor = StudioColors.CardDark;
            dgv.DefaultCellStyle.ForeColor = StudioColors.TextWhite;
            dgv.DefaultCellStyle.SelectionBackColor = StudioColors.PrimaryPurple;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = StudioColors.RegularFont;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 48);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = StudioColors.TextWhite;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = StudioColors.PrimaryPurple;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            return dgv;
        }

        private void SetupGridColumns()
        {
            dgvMembers.Columns.Clear();
            dgvMembers.AutoGenerateColumns = false;

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Id", 
                HeaderText = "ID", 
                Name = "colId", 
                Width = 45, 
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "HoTen", 
                HeaderText = "Họ và Tên", 
                Name = "colHoTen", 
                Width = 220,
                ReadOnly = true,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9f, FontStyle.Bold) }
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "TenDangNhap", 
                HeaderText = "Username", 
                Name = "colTenDangNhap", 
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = { ForeColor = StudioColors.TextMuted }
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "VaiTroChuyenMon", 
                HeaderText = "Vai Trò Chuyên Môn", 
                Name = "colVaiTro", 
                Width = 145,
                ReadOnly = true
            });

            var colStatus = new DataGridViewComboBoxColumn 
            { 
                DataPropertyName = "TrangThaiHoatDong", 
                HeaderText = "Trạng Thái", 
                Name = "colTrangThai", 
                Width = 125,
                FlatStyle = FlatStyle.Flat,
                ReadOnly = false,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            colStatus.Items.AddRange(MemberStatuses.All);
            colStatus.DefaultCellStyle.BackColor = StudioColors.SidebarDark;
            colStatus.DefaultCellStyle.ForeColor = StudioColors.SafeGreen;
            colStatus.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMembers.Columns.Add(colStatus);

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Email", 
                HeaderText = "Email", 
                Name = "colEmail", 
                Width = 225,
                ReadOnly = true
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "SoDienThoai", 
                HeaderText = "Số Điện Thoại", 
                Name = "colPhone", 
                Width = 115,
                ReadOnly = true
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "DiscordTag", 
                HeaderText = "Discord", 
                Name = "colDiscord", 
                Width = 165,
                ReadOnly = true
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "ActiveTasks", 
                HeaderText = "Đang Làm", 
                Name = "colActiveTasks", 
                Width = 85,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "OverdueTasks", 
                HeaderText = "Trễ Hạn", 
                Name = "colOverdueTasks", 
                Width = 85,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "WorkloadLevel", 
                HeaderText = "Tải Công Việc", 
                Name = "colWorkload", 
                Width = 115,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
            });

            var colEdit = new DataGridViewButtonColumn 
            { 
                HeaderText = "Thao Tác", 
                Name = "colEdit", 
                Width = 65, 
                Text = "Sửa", 
                ReadOnly = true,
                UseColumnTextForButtonValue = true 
            };
            dgvMembers.Columns.Add(colEdit);

            var colDelete = new DataGridViewButtonColumn 
            { 
                HeaderText = "", 
                Name = "colDelete", 
                Width = 65, 
                Text = "Xóa", 
                ReadOnly = true,
                UseColumnTextForButtonValue = true 
            };
            dgvMembers.Columns.Add(colDelete);
        }

        private void DgvMembers_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvMembers.Columns[e.ColumnIndex].Name == "colTrangThai")
            {
                if (e.RowIndex < _currentRows.Count)
                {
                    var row = _currentRows[e.RowIndex];
                    string? newStatus = dgvMembers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                    if (!string.IsNullOrEmpty(newStatus) && newStatus != row.TrangThaiHoatDong)
                    {
                        row.TrangThaiHoatDong = newStatus;
                        row.Dto.Member.TrangThaiHoatDong = newStatus;
                        _memberService.UpdateMemberStatus(row.Id, newStatus);
                    }
                }
            }
        }

        public void LoadMembers()
        {
            string? kw = txtSearch.Text.Trim();
            string? role = cboRoleFilter.SelectedItem?.ToString();
            string? status = cboStatusFilter.SelectedItem?.ToString();

            var list = _memberService.GetMembersWithWorkload();

            if (!string.IsNullOrWhiteSpace(kw))
            {
                string k = kw.ToLower();
                list = list.FindAll(w => w.Member.HoTen.ToLower().Contains(k) ||
                                         w.Member.Email.ToLower().Contains(k) ||
                                         w.Member.TenDangNhap.ToLower().Contains(k) ||
                                         (w.Member.DiscordTag != null && w.Member.DiscordTag.ToLower().Contains(k)));
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "Tất cả")
            {
                list = list.FindAll(w => w.Member.VaiTroChuyenMon == role);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả")
            {
                list = list.FindAll(w => w.Member.TrangThaiHoatDong == status);
            }

            _currentRows = list.Select(w => new MemberTableRowModel
            {
                Id = w.Member.Id,
                HoTen = w.Member.HoTen,
                TenDangNhap = w.Member.TenDangNhap,
                VaiTroChuyenMon = w.Member.VaiTroChuyenMon,
                TrangThaiHoatDong = w.Member.TrangThaiHoatDong,
                Email = w.Member.Email,
                SoDienThoai = string.IsNullOrWhiteSpace(w.Member.SoDienThoai) ? "-" : w.Member.SoDienThoai,
                DiscordTag = string.IsNullOrWhiteSpace(w.Member.DiscordTag) ? "-" : w.Member.DiscordTag,
                ActiveTasks = w.ActiveTasks,
                OverdueTasks = w.OverdueTasks,
                WorkloadLevel = w.WorkloadLevel,
                Dto = w
            }).ToList();

            dgvMembers.DataSource = null;
            dgvMembers.DataSource = _currentRows;
            lblCount.Text = $"Tổng số: {_currentRows.Count} thành viên";
        }

        private void DgvMembers_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentRows.Count) return;

            string colName = dgvMembers.Columns[e.ColumnIndex].Name;
            if (colName == "colEdit")
            {
                var row = _currentRows[e.RowIndex];
                var dlg = new Dialogs.MemberEditDialog(row.Dto.Member);
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadMembers();
            }
            else if (colName == "colDelete")
            {
                var row = _currentRows[e.RowIndex];
                DeleteMemberPrompt(row.Dto.Member);
            }
        }

        private void DgvMembers_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentRows.Count) return;
            var row = _currentRows[e.RowIndex];
            var dlg = new Dialogs.MemberEditDialog(row.Dto.Member);
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadMembers();
        }

        private void DgvMembers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null || e.CellStyle == null) return;

            string colName = dgvMembers.Columns[e.ColumnIndex].Name;
            if (colName == "colTrangThai")
            {
                string val = e.Value.ToString() ?? "";
                if (val == MemberStatuses.Active)
                    e.CellStyle.ForeColor = StudioColors.SafeGreen;
                else if (val == MemberStatuses.Busy)
                    e.CellStyle.ForeColor = StudioColors.WarningAmber;
                else
                    e.CellStyle.ForeColor = StudioColors.TextMuted;
            }
            else if (colName == "colVaiTro")
            {
                e.CellStyle.ForeColor = StudioColors.CyanNeon;
            }
            else if (colName == "colOverdueTasks")
            {
                if (e.Value is int overdue && overdue > 0)
                {
                    e.CellStyle.ForeColor = StudioColors.DangerRed;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            }
            else if (colName == "colWorkload")
            {
                string val = e.Value.ToString() ?? "";
                if (val.Contains("Quá tải"))
                    e.CellStyle.ForeColor = StudioColors.DangerRed;
                else if (val.Contains("Bận"))
                    e.CellStyle.ForeColor = StudioColors.WarningAmber;
                else
                    e.CellStyle.ForeColor = StudioColors.SafeGreen;
            }
        }

        private void DgvMembers_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics == null) return;

            string colName = dgvMembers.Columns[e.ColumnIndex].Name;
            if (colName == "colEdit" || colName == "colDelete")
            {
                e.PaintBackground(e.CellBounds, true);

                bool isEdit = colName == "colEdit";
                string btnText = isEdit ? "Sửa" : "Xóa";
                Color btnBg = isEdit ? Color.FromArgb(55, 45, 80) : Color.FromArgb(65, 30, 40);
                Color textCol = isEdit ? Color.White : StudioColors.DangerRed;

                var btnRect = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 5, e.CellBounds.Width - 8, e.CellBounds.Height - 10);
                using (var path = RoundedPanel.GetRoundedPath(btnRect, 4))
                using (var brush = new SolidBrush(btnBg))
                using (var pen = new Pen(isEdit ? StudioColors.PrimaryPurple : StudioColors.DangerRed, 1))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }

                using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, btnText, font, btnRect, textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        private void DeleteMemberPrompt(ThanhVien m)
        {
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa thành viên '{m.HoTen}' ({m.VaiTroChuyenMon})?",
                "Xác nhận xóa thành viên", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var res = _memberService.DeleteMember(m.Id);
                if (!res.Success)
                {
                    MessageBox.Show(res.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    LoadMembers();
                }
            }
        }
    }

    public class MemberTableRowModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public string VaiTroChuyenMon { get; set; } = string.Empty;
        public string TrangThaiHoatDong { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiscordTag { get; set; } = string.Empty;
        public int ActiveTasks { get; set; }
        public int OverdueTasks { get; set; }
        public string WorkloadLevel { get; set; } = string.Empty;
        public MemberWorkloadDto Dto { get; set; } = null!;
    }
}
