using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Dialogs
{
    public class NotificationCenterDialog : Form
    {
        private readonly NotificationService _notificationService = new();
        private FlowLayoutPanel pnlList = null!;

        public NotificationCenterDialog()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private void InitializeComponent()
        {
            Text = "Trung Tâm Thông Báo (Notification Center)";
            Size = new Size(520, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(16, 12, 16, 12),
                BackColor = StudioColors.SidebarDark,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = "THÔNG BÁO & NHẮC HẸN",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = StudioColors.PrimaryPurple,
                Location = new Point(16, 16),
                AutoSize = true,
                Parent = pnlTop
            };

            var btnReadAll = new RoundedButton
            {
                Text = "Đã đọc tất cả",
                Location = new Point(360, 12),
                Size = new Size(130, 36),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.PrimaryPurple,
                Parent = pnlTop
            };
            btnReadAll.Click += (s, e) =>
            {
                _notificationService.MarkAllAsRead(AuthService.CurrentUser?.Id);
                LoadNotifications();
            };

            pnlList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(16),
                BackColor = StudioColors.BackgroundDark,
                Parent = this
            };
        }

        private void LoadNotifications()
        {
            pnlList.Controls.Clear();
            var list = _notificationService.GetNotificationsForUser(AuthService.CurrentUser?.Id);

            if (list.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Không có thông báo nào.",
                    Font = StudioColors.RegularFont,
                    ForeColor = StudioColors.TextMuted,
                    Size = new Size(460, 40),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlList.Controls.Add(lblEmpty);
                return;
            }

            foreach (var notif in list)
            {
                Color borderCol = notif.MucDo switch
                {
                    "Danger" => StudioColors.DangerRed,
                    "Warning" => StudioColors.WarningAmber,
                    _ => StudioColors.CyanNeon
                };

                var itemPanel = new RoundedPanel
                {
                    Size = new Size(460, 85),
                    BorderRadius = 10,
                    CustomBackgroundColor = notif.DaDoc ? StudioColors.SidebarDark : StudioColors.CardDark,
                    BorderColor = borderCol,
                    BorderSize = notif.DaDoc ? 1 : 2,
                    Margin = new Padding(0, 0, 0, 10),
                    Padding = new Padding(12, 10, 12, 10),
                    Cursor = Cursors.Hand
                };

                // Time ago
                var lblTime = new Label
                {
                    Text = $"{notif.TimeAgo} • {notif.LoaiThongBao}",
                    Font = StudioColors.SmallFont,
                    ForeColor = StudioColors.TextMuted,
                    Dock = DockStyle.Top,
                    Height = 16
                };

                // Title
                var lblItemTitle = new Label
                {
                    Text = notif.TieuDe,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = notif.DaDoc ? StudioColors.TextSecondary : StudioColors.TextWhite,
                    Dock = DockStyle.Top,
                    Height = 22
                };

                // Content
                var lblContent = new Label
                {
                    Text = notif.NoiDung,
                    Font = StudioColors.SmallFont,
                    ForeColor = StudioColors.TextMuted,
                    Dock = DockStyle.Fill
                };

                itemPanel.Controls.Add(lblContent);
                itemPanel.Controls.Add(lblItemTitle);
                itemPanel.Controls.Add(lblTime);

                itemPanel.Click += (s, e) =>
                {
                    if (!notif.DaDoc)
                    {
                        _notificationService.MarkAsRead(notif.Id);
                        notif.DaDoc = true;
                        itemPanel.CustomBackgroundColor = StudioColors.SidebarDark;
                        itemPanel.BorderSize = 1;
                        itemPanel.Invalidate();
                    }
                };

                pnlList.Controls.Add(itemPanel);
            }
        }
    }
}
