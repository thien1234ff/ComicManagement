using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;

namespace ComicManagement.Forms.Dialogs
{
    public class ToastNotificationForm : Form
    {
        private System.Windows.Forms.Timer _timer = null!;
        private int _ticks = 0;

        public ToastNotificationForm(List<ThongBao> urgentNotifs)
        {
            InitializeComponent(urgentNotifs);
        }

        private void InitializeComponent(List<ThongBao> urgentNotifs)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(380, Math.Min(320, 90 + urgentNotifs.Count * 65));
            BackColor = StudioColors.SidebarDark;

            // Đặt vị trí góc dưới bên phải màn hình
            var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            Location = new Point(workingArea.Right - Width - 24, workingArea.Bottom - Height - 24);

            // Container Panel với viền đỏ cảnh báo
            var pnl = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 14,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.DangerRed,
                BorderSize = 2,
                Padding = new Padding(12),
                Parent = this
            };

            // Header toast
            var lblTitle = new Label
            {
                Text = $"CẢNH BÁO TIẾN ĐỘ KHẨN CẤP ({urgentNotifs.Count})",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = StudioColors.DangerRed,
                Location = new Point(14, 12),
                Size = new Size(310, 24),
                Parent = pnl
            };

            var btnClose = new Label
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = StudioColors.TextMuted,
                Location = new Point(320, 10),
                Size = new Size(45, 24),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = pnl
            };
            btnClose.Click += (s, e) => Close();

            // List of items
            var pnlList = new FlowLayoutPanel
            {
                Location = new Point(12, 42),
                Size = new Size(356, Height - 52),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Parent = pnl
            };

            foreach (var notif in urgentNotifs)
            {
                var itemCard = new RoundedPanel
                {
                    Size = new Size(330, 56),
                    BorderRadius = 8,
                    CustomBackgroundColor = Color.FromArgb(40, 30, 45),
                    BorderColor = (notif.MucDo == "Danger") ? StudioColors.DangerRed : StudioColors.WarningAmber,
                    Margin = new Padding(0, 0, 0, 6),
                    Padding = new Padding(8, 6, 8, 6)
                };

                var lblItemTitle = new Label
                {
                    Text = notif.TieuDe,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = StudioColors.TextWhite,
                    Dock = DockStyle.Top,
                    Height = 20
                };

                var lblItemContent = new Label
                {
                    Text = notif.NoiDung,
                    Font = StudioColors.SmallFont,
                    ForeColor = StudioColors.TextMuted,
                    Dock = DockStyle.Fill
                };

                itemCard.Controls.Add(lblItemContent);
                itemCard.Controls.Add(lblItemTitle);
                pnlList.Controls.Add(itemCard);
            }

            // Tự động đóng sau 10 giây
            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += (s, e) =>
            {
                _ticks++;
                if (_ticks >= 10)
                {
                    _timer.Stop();
                    Close();
                }
            };
            _timer.Start();
        }
    }
}
