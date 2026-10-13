using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Dialogs
{
    public class QuickProgressDialog : Form
    {
        private readonly DeadlineService _deadlineService = new();
        private readonly CongDoanDeadline _deadline;

        private ComboBox cboStatus = null!;
        private TextBox txtLink = null!;

        public QuickProgressDialog(CongDoanDeadline deadline)
        {
            _deadline = deadline;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"Cập Nhật Nhanh: {_deadline.TenCongDoan}";
            Size = new Size(460, 320);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(400, 240),
                Location = new Point(22, 18),
                BorderRadius = 14,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTask = new Label
            {
                Text = _deadline.ComicAndChapterName,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(20, 16),
                Size = new Size(360, 22),
                Parent = card
            };

            var lblStage = new Label
            {
                Text = $"Công đoạn: {_deadline.TenCongDoan}",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(20, 38),
                Size = new Size(360, 18),
                Parent = card
            };

            // Status
            var lblStt = new Label
            {
                Text = "Trạng thái mới:",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(20, 68),
                Size = new Size(360, 18),
                Parent = card
            };

            var pnlStatus = new RoundedPanel
            {
                Location = new Point(20, 88),
                Size = new Size(360, 36),
                BorderRadius = 6,
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
            cboStatus.Items.AddRange(TaskStatuses.AllStatuses);
            cboStatus.SelectedItem = _deadline.TrangThai;

            // Link Nộp bài
            var lblLink = new Label
            {
                Text = "Link sản phẩm hoàn thành (Drive / Dropbox / OneDrive):",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(20, 130),
                Size = new Size(360, 18),
                Parent = card
            };

            var pnlLink = new RoundedPanel
            {
                Location = new Point(20, 150),
                Size = new Size(360, 36),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(8, 6, 8, 6),
                Parent = card
            };
            txtLink = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                Text = _deadline.LinkNopSanPham ?? "",
                Parent = pnlLink
            };

            // Buttons
            var btnSave = new RoundedButton
            {
                Text = "LƯU TIẾN ĐỘ",
                Location = new Point(20, 196),
                Size = new Size(175, 34),
                BorderRadius = 6,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = card
            };
            btnSave.Click += (s, e) =>
            {
                string newStatus = cboStatus.SelectedItem?.ToString() ?? TaskStatuses.InProgress;
                _deadlineService.QuickUpdateProgress(_deadline.Id, newStatus, txtLink.Text.Trim());
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new RoundedButton
            {
                Text = "Hủy bỏ",
                Location = new Point(205, 196),
                Size = new Size(175, 34),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnCancel.Click += (s, e) => Close();
        }
    }
}
