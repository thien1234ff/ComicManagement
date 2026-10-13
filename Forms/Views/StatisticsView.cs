using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Services;

namespace ComicManagement.Forms.Views
{
    public class StatisticsView : UserControl
    {
        private readonly StatisticsService _statsService = new();

        private StudioChartControl chartWeekly = null!;
        private StudioChartControl chartStages = null!;
        private DataGridView dgvLeaderboard = null!;

        public StatisticsView()
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
                Padding = new Padding(20, 16, 20, 16),
                Parent = this
            };

            // Header Banner
            var banner = new RoundedPanel
            {
                Location = new Point(16, 12),
                Size = new Size(1070, 66),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(16, 10, 16, 10),
                Parent = pnlContainer
            };

            var lblTitle = new Label
            {
                Text = "BÁO CÁO NĂNG SUẤT & HIỆU QUẢ BIÊN TẬP",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(14, 10),
                AutoSize = true,
                Parent = banner
            };

            var lblSub = new Label
            {
                Text = "Xếp hạng đóng góp thành viên, tỷ lệ giao nộp đúng hạn (%) và phân tích sản lượng công đoạn.",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(14, 34),
                AutoSize = true,
                Parent = banner
            };

            // Charts Section
            int chartY = 90;
            int chartWidth = 527;
            int spacing = 16;

            chartWeekly = new StudioChartControl
            {
                ChartTitle = "Số Task hoàn thành theo tuần",
                Location = new Point(16, chartY),
                Size = new Size(chartWidth, 240),
                Parent = pnlContainer
            };

            chartStages = new StudioChartControl
            {
                ChartTitle = "Tỷ lệ các công đoạn đang thực hiện",
                Location = new Point(16 + chartWidth + spacing, chartY),
                Size = new Size(chartWidth, 240),
                Parent = pnlContainer
            };

            // Leaderboard Section
            int leadY = 345;
            var pnlLeadHead = new Panel
            {
                Location = new Point(16, leadY),
                Size = new Size(1070, 32),
                Parent = pnlContainer
            };

            var lblLeadTitle = new Label
            {
                Text = "BẢNG XẾP HẠNG ĐÓNG GÓP & TỶ LỆ HOÀN THÀNH ĐÚNG HẠN",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = StudioColors.SecondaryPink,
                Location = new Point(0, 4),
                AutoSize = true,
                Parent = pnlLeadHead
            };

            var pnlGridWrap = new RoundedPanel
            {
                Location = new Point(16, leadY + 36),
                Size = new Size(1070, 320),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10),
                Parent = pnlContainer
            };

            dgvLeaderboard = new DataGridView
            {
                Dock = DockStyle.Fill,
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
                RowTemplate = { Height = 36 },
                Parent = pnlGridWrap
            };

            dgvLeaderboard.ColumnHeadersDefaultCellStyle.BackColor = StudioColors.SidebarDark;
            dgvLeaderboard.ColumnHeadersDefaultCellStyle.ForeColor = StudioColors.CyanNeon;
            dgvLeaderboard.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvLeaderboard.ColumnHeadersHeight = 36;

            dgvLeaderboard.DefaultCellStyle.BackColor = StudioColors.CardDark;
            dgvLeaderboard.DefaultCellStyle.ForeColor = StudioColors.TextWhite;
            dgvLeaderboard.DefaultCellStyle.SelectionBackColor = StudioColors.PrimaryPurple;
            dgvLeaderboard.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvLeaderboard.DefaultCellStyle.Font = StudioColors.RegularFont;

            dgvLeaderboard.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MemberName", HeaderText = "Tên Thành Viên", Width = 260 });
            dgvLeaderboard.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Chuyên Môn", Width = 180 });
            dgvLeaderboard.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CompletedTasksCount", HeaderText = "Tasks Đã Xong", Width = 160 });
            dgvLeaderboard.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAssignedCount", HeaderText = "Tổng Task Giao", Width = 160 });
            dgvLeaderboard.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OnTimeRatePercent", HeaderText = "Đúng Hạn (%)", Width = 160 });
            dgvLeaderboard.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OverdueCount", HeaderText = "Số Task Trễ", Width = 160 });
        }

        public void LoadData()
        {
            chartWeekly.Data = _statsService.GetWeeklyProgressChartData();
            chartStages.Data = _statsService.GetStageDistributionChartData();

            var leaderboard = _statsService.GetMemberLeaderboard();
            dgvLeaderboard.DataSource = leaderboard;
        }
    }
}
