using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Views
{
    public class DashboardView : UserControl
    {
        private readonly StatisticsService _statsService = new();
        private readonly DeadlineService _deadlineService = new();

        private Label lblValComics = null!;
        private Label lblValChapters = null!;
        private Label lblValOverdue = null!;
        private Label lblValUpcoming = null!;
        private StudioChartControl chartWeekly = null!;
        private StudioChartControl chartStages = null!;
        private FlowLayoutPanel pnlUrgentList = null!;

        public event Action? NavigateToDeadlines;

        public DashboardView()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = StudioColors.BackgroundDark;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(16, 12, 16, 16),
                Parent = this
            };

            // 1. Header Banner
            var banner = new RoundedPanel
            {
                Location = new Point(16, 12),
                Size = new Size(1070, 66),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Padding = new Padding(16, 10, 16, 10),
                Parent = pnlContainer
            };

            var lblGreeting = new Label
            {
                Text = $"Chào mừng, {AuthService.CurrentUser?.HoTen ?? "Biên tập viên"} ({AuthService.CurrentUser?.VaiTroChuyenMon ?? "Studio"})",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                Location = new Point(14, 10),
                AutoSize = true,
                Parent = banner
            };

            var lblDesc = new Label
            {
                Text = "Theo dõi tiến độ biên tập, cảnh báo deadline và thống kê năng suất toàn hệ thống.",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(14, 34),
                AutoSize = true,
                Parent = banner
            };

            // 2. 4 Metrics Summary Cards
            int cardY = 88;
            int cardWidth = 255;
            int spacing = 16;

            var card1 = CreateMetricCard(
                "TRUYỆN ĐANG LÀM", "0", "Bộ truyện đang biên tập",
                StudioColors.CyanNeon, new Point(16, cardY), cardWidth, out lblValComics);
            pnlContainer.Controls.Add(card1);

            var card2 = CreateMetricCard(
                "CHAPTER THÁNG NÀY", "0", "Đã xuất bản thành công",
                StudioColors.SafeGreen, new Point(16 + (cardWidth + spacing), cardY), cardWidth, out lblValChapters);
            pnlContainer.Controls.Add(card2);

            var card3 = CreateMetricCard(
                "QUÁ HẠN (OVERDUE)", "0", "Cần xử lý khẩn cấp",
                StudioColors.DangerRed, new Point(16 + (cardWidth + spacing) * 2, cardY), cardWidth, out lblValOverdue);
            pnlContainer.Controls.Add(card3);

            var card4 = CreateMetricCard(
                "SẮP HẾT HẠN (48H)", "0", "Ưu tiên hoàn thiện sớm",
                StudioColors.WarningAmber, new Point(16 + (cardWidth + spacing) * 3, cardY), cardWidth, out lblValUpcoming);
            pnlContainer.Controls.Add(card4);

            // 3. Charts Area (2 Charts cân xứng)
            int chartY = 200;
            int chartWidth = 527;
            chartWeekly = new StudioChartControl
            {
                ChartTitle = "Tiến độ hoàn thành Task 7 ngày gần nhất",
                Location = new Point(16, chartY),
                Size = new Size(chartWidth, 240),
                Parent = pnlContainer
            };

            chartStages = new StudioChartControl
            {
                ChartTitle = "Phân bổ công đoạn đang thực hiện",
                Location = new Point(16 + chartWidth + spacing, chartY),
                Size = new Size(chartWidth, 240),
                Parent = pnlContainer
            };

            // 4. Urgent Tasks List
            int urgentY = 454;
            var pnlUrgentHeader = new Panel
            {
                Location = new Point(16, urgentY),
                Size = new Size(1070, 34),
                Parent = pnlContainer
            };

            var lblUrgentTitle = new Label
            {
                Text = "DEADLINE CẦN XỬ LÝ GẤP",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                Location = new Point(0, 4),
                AutoSize = true,
                Parent = pnlUrgentHeader
            };

            var btnViewAllDeadlines = new RoundedButton
            {
                Text = "Xem Pipeline Kanban",
                Location = new Point(895, 0),
                Size = new Size(175, 32),
                BorderRadius = 6,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.PrimaryPurple,
                Parent = pnlUrgentHeader
            };
            btnViewAllDeadlines.Click += (s, e) => NavigateToDeadlines?.Invoke();

            pnlUrgentList = new FlowLayoutPanel
            {
                Location = new Point(16, urgentY + 38),
                Size = new Size(1070, 320),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Parent = pnlContainer
            };
        }

        private RoundedPanel CreateMetricCard(string title, string value, string subtext, Color accent, Point loc, int width, out Label valLabel)
        {
            var pnl = new RoundedPanel
            {
                Location = loc,
                Size = new Size(width, 98),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Padding = new Padding(14, 10, 14, 10)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(12, 10),
                Size = new Size(width - 24, 16),
                Parent = pnl
            };

            valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = StudioColors.TextWhite,
                Location = new Point(10, 26),
                Size = new Size(width - 24, 38),
                Parent = pnl
            };

            var lblSub = new Label
            {
                Text = subtext,
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(12, 68),
                Size = new Size(width - 24, 16),
                Parent = pnl
            };

            return pnl;
        }

        public void LoadData()
        {
            var metrics = _statsService.GetDashboardMetrics();
            lblValComics.Text = metrics.TotalActiveComics.ToString();
            lblValChapters.Text = metrics.PublishedChaptersThisMonth.ToString();
            lblValOverdue.Text = metrics.TotalOverdueDeadlines.ToString();
            lblValUpcoming.Text = metrics.UpcomingDeadlines48h.ToString();

            // Load Charts
            chartWeekly.Data = _statsService.GetWeeklyProgressChartData();
            chartStages.Data = _statsService.GetStageDistributionChartData();

            // Load Urgent Deadline Cards
            pnlUrgentList.Controls.Clear();
            var urgentDeadlines = _deadlineService.GetDeadlines()
                .FindAll(d => d.Urgency == DeadlineUrgency.Overdue || d.Urgency == DeadlineUrgency.Warning);

            if (urgentDeadlines.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Hiện tại không có deadline nào quá hạn hoặc sắp trễ.",
                    Font = StudioColors.RegularFont,
                    ForeColor = StudioColors.SafeGreen,
                    Size = new Size(800, 36),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                pnlUrgentList.Controls.Add(lblEmpty);
            }
            else
            {
                foreach (var d in urgentDeadlines)
                {
                    var card = new DeadlineItemCard
                    {
                        Deadline = d,
                        Width = 255,
                        Height = 130,
                        Margin = new Padding(0, 0, 14, 12)
                    };
                    card.QuickActionClicked += (s, task) =>
                    {
                        var dlg = new Dialogs.QuickProgressDialog(task);
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadData();
                    };
                    card.EditClicked += (s, task) =>
                    {
                        var dlg = new Dialogs.DeadlineEditDialog(task);
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK) LoadData();
                    };
                    pnlUrgentList.Controls.Add(card);
                }
            }
        }
    }
}
