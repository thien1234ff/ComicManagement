using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Forms.Dialogs;
using ComicManagement.Forms.Views;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms
{
    public class MainDashboardForm : Form
    {
        private readonly NotificationService _notificationService = new();
        private readonly AuthService _authService = new();

        private Panel pnlSidebar = null!;
        private Panel pnlRightContainer = null!;
        private Panel pnlHeader = null!;
        private Panel pnlMainContent = null!;
        private Label lblHeaderTitle = null!;
        private RoundedButton btnNotifBell = null!;

        // Sidebar Navigation Buttons
        private RoundedButton btnNavDashboard = null!;
        private RoundedButton btnNavComics = null!;
        private RoundedButton btnNavDeadlines = null!;
        private RoundedButton btnNavMembers = null!;
        private RoundedButton btnNavStats = null!;
        private RoundedButton btnLogout = null!;

        // Sub Views
        private DashboardView? _dashboardView;
        private ComicManagementView? _comicsView;
        private DeadlineKanbanView? _deadlinesView;
        private MemberManagementView? _membersView;
        private StatisticsView? _statsView;

        public MainDashboardForm()
        {
            InitializeComponent();
            CheckUrgentNotificationsAndPopups();
            ShowView("Dashboard");
        }

        private void InitializeComponent()
        {
            Text = "Manga / Webtoon Creative Editorial Studio Hub";
            Size = new Size(1380, 850);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            // 1. Sidebar (Trái - Cố định 230px, Dock Left)
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = StudioColors.SidebarDark,
                Parent = this
            };

            SetupSidebar();

            // 2. Container Bên Phải (Chứa Header và MainContent - Tách biệt hoàn toàn khỏi Sidebar)
            pnlRightContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.BackgroundDark,
                Parent = this
            };

            // 3. Header (Nằm trên đỉnh của pnlRightContainer)
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = StudioColors.SidebarDark,
                Padding = new Padding(20, 10, 20, 10),
                Parent = pnlRightContainer
            };

            SetupHeader();

            // 4. Main Content (Nằm trong phần còn lại của pnlRightContainer)
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.BackgroundDark,
                Parent = pnlRightContainer
            };

            // Đảm bảo thứ tự Z-order chuẩn xác trong WinForms
            pnlSidebar.BringToFront();
            pnlRightContainer.BringToFront();
            pnlHeader.BringToFront();
            pnlMainContent.BringToFront();
        }

        private void SetupSidebar()
        {
            pnlSidebar.Controls.Clear();

            // Logo & Subtitle
            var lblBrand = new Label
            {
                Text = "STUDIO HUB",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = StudioColors.PrimaryPurple,
                Location = new Point(16, 16),
                Size = new Size(198, 26),
                Parent = pnlSidebar
            };

            var lblSubBrand = new Label
            {
                Text = "Hệ thống biên tập truyện tranh",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(16, 42),
                Size = new Size(198, 18),
                Parent = pnlSidebar
            };

            // User Info Box
            var pnlUserCard = new RoundedPanel
            {
                Location = new Point(16, 68),
                Size = new Size(198, 60),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Padding = new Padding(8, 6, 8, 6),
                Parent = pnlSidebar
            };

            var u = AuthService.CurrentUser;
            string userName = u?.HoTen ?? "Biên tập viên";
            string userRole = u?.VaiTroChuyenMon ?? StudioRoles.Translator;

            var lblUserRole = new Label
            {
                Text = userRole,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(10, 8),
                Size = new Size(178, 16),
                Parent = pnlUserCard
            };

            var lblUserName = new Label
            {
                Text = userName,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                Location = new Point(10, 26),
                Size = new Size(178, 22),
                Parent = pnlUserCard
            };

            // Menu Buttons
            int startY = 144;
            int btnHeight = 40;
            int spacing = 6;
            int btnWidth = 198;

            btnNavDashboard = CreateSidebarButton("Tổng Quan", new Point(16, startY), new Size(btnWidth, btnHeight), () => ShowView("Dashboard"));
            btnNavComics = CreateSidebarButton("Quản Lý Truyện", new Point(16, startY + (btnHeight + spacing)), new Size(btnWidth, btnHeight), () => ShowView("Comics"));
            btnNavDeadlines = CreateSidebarButton("Quản Lý Deadline", new Point(16, startY + (btnHeight + spacing) * 2), new Size(btnWidth, btnHeight), () => ShowView("Deadlines"));
            btnNavMembers = CreateSidebarButton("Quản Lý Thành Viên", new Point(16, startY + (btnHeight + spacing) * 3), new Size(btnWidth, btnHeight), () => ShowView("Members"));
            btnNavStats = CreateSidebarButton("Thống Kê & Báo Cáo", new Point(16, startY + (btnHeight + spacing) * 4), new Size(btnWidth, btnHeight), () => ShowView("Stats"));

            pnlSidebar.Controls.Add(btnNavDashboard);
            pnlSidebar.Controls.Add(btnNavComics);
            pnlSidebar.Controls.Add(btnNavDeadlines);
            pnlSidebar.Controls.Add(btnNavMembers);
            pnlSidebar.Controls.Add(btnNavStats);

            // Nút Đăng xuất ở chân Sidebar
            btnLogout = new RoundedButton
            {
                Text = "Đăng Xuất",
                Size = new Size(btnWidth, 38),
                Location = new Point(16, pnlSidebar.Height - 54),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 35, 50),
                HoverColor = StudioColors.DangerRed,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Parent = pnlSidebar
            };
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _authService.Logout();
                    Hide();
                    var login = new LoginForm();
                    login.FormClosed += (sender, args) => Close();
                    login.Show();
                }
            };
        }

        private RoundedButton CreateSidebarButton(string text, Point loc, Size size, Action onClick)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Location = loc,
                Size = size,
                BorderRadius = 6,
                NormalColor = StudioColors.SidebarDark,
                HoverColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextSecondary,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void SetupHeader()
        {
            lblHeaderTitle = new Label
            {
                Text = "BẢNG ĐIỀU KHIỂN TỔNG QUAN",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                Location = new Point(20, 16),
                AutoSize = true,
                Parent = pnlHeader
            };

            btnNotifBell = new RoundedButton
            {
                Text = "Thông Báo",
                Location = new Point(pnlHeader.Width - 145, 10),
                Size = new Size(125, 34),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.PrimaryPurple,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Parent = pnlHeader
            };
            btnNotifBell.Click += (s, e) =>
            {
                var dlg = new NotificationCenterDialog();
                dlg.ShowDialog(this);
                UpdateNotificationBadge();
            };

            UpdateNotificationBadge();
        }

        private void UpdateNotificationBadge()
        {
            int unread = _notificationService.GetUnreadCount(AuthService.CurrentUser?.Id);
            if (unread > 0)
            {
                btnNotifBell.Text = $"Thông Báo ({unread})";
                btnNotifBell.NormalColor = StudioColors.DangerRed;
                btnNotifBell.HoverColor = Color.FromArgb(248, 113, 113);
            }
            else
            {
                btnNotifBell.Text = "Thông Báo";
                btnNotifBell.NormalColor = Color.FromArgb(45, 45, 65);
                btnNotifBell.HoverColor = StudioColors.PrimaryPurple;
            }
        }

        private void CheckUrgentNotificationsAndPopups()
        {
            _notificationService.AutoScanAndGenerateDeadlineAlerts();

            var urgentList = _notificationService.GetUrgentTasksForLoginPopup(AuthService.CurrentUser?.Id);
            if (urgentList.Count > 0)
            {
                var toast = new ToastNotificationForm(urgentList);
                toast.Show();
            }

            UpdateNotificationBadge();
        }

        private void ShowView(string viewName)
        {
            pnlMainContent.Controls.Clear();
            ResetNavButtons();

            switch (viewName)
            {
                case "Dashboard":
                    lblHeaderTitle.Text = "BẢNG ĐIỀU KHIỂN TỔNG QUAN";
                    HighlightNavButton(btnNavDashboard);
                    _dashboardView ??= new DashboardView();
                    _dashboardView.NavigateToDeadlines += () => ShowView("Deadlines");
                    _dashboardView.LoadData();
                    pnlMainContent.Controls.Add(_dashboardView);
                    break;

                case "Comics":
                    lblHeaderTitle.Text = "QUẢN LÝ BỘ TRUYỆN & CHAPTERS";
                    HighlightNavButton(btnNavComics);
                    _comicsView ??= new ComicManagementView();
                    _comicsView.LoadComics();
                    pnlMainContent.Controls.Add(_comicsView);
                    break;

                case "Deadlines":
                    lblHeaderTitle.Text = "QUẢN LÝ DEADLINE (PIPELINE KANBAN)";
                    HighlightNavButton(btnNavDeadlines);
                    _deadlinesView ??= new DeadlineKanbanView();
                    _deadlinesView.LoadKanbanBoards();
                    pnlMainContent.Controls.Add(_deadlinesView);
                    break;

                case "Members":
                    lblHeaderTitle.Text = "ĐỘI NGŨ THÀNH VIÊN & TẢI CÔNG VIỆC";
                    HighlightNavButton(btnNavMembers);
                    _membersView ??= new MemberManagementView();
                    _membersView.LoadMembers();
                    pnlMainContent.Controls.Add(_membersView);
                    break;

                case "Stats":
                    lblHeaderTitle.Text = "BÁO CÁO NĂNG SUẤT & PHÂN TÍCH TIẾN ĐỘ";
                    HighlightNavButton(btnNavStats);
                    _statsView ??= new StatisticsView();
                    _statsView.LoadData();
                    pnlMainContent.Controls.Add(_statsView);
                    break;
            }
        }

        private void ResetNavButtons()
        {
            btnNavDashboard.NormalColor = StudioColors.SidebarDark;
            btnNavDashboard.ForeColor = StudioColors.TextSecondary;
            btnNavComics.NormalColor = StudioColors.SidebarDark;
            btnNavComics.ForeColor = StudioColors.TextSecondary;
            btnNavDeadlines.NormalColor = StudioColors.SidebarDark;
            btnNavDeadlines.ForeColor = StudioColors.TextSecondary;
            btnNavMembers.NormalColor = StudioColors.SidebarDark;
            btnNavMembers.ForeColor = StudioColors.TextSecondary;
            btnNavStats.NormalColor = StudioColors.SidebarDark;
            btnNavStats.ForeColor = StudioColors.TextSecondary;
        }

        private void HighlightNavButton(RoundedButton btn)
        {
            btn.NormalColor = StudioColors.PrimaryPurple;
            btn.ForeColor = StudioColors.TextWhite;
        }
    }
}
