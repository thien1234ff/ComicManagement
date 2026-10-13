using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ComicManagement.Services;

namespace ComicManagement.Controls
{
    public class StudioChartControl : UserControl
    {
        public string ChartTitle { get; set; } = "Biểu đồ tiến độ";
        private List<ChartDataPoint> _data = new();

        public List<ChartDataPoint> Data
        {
            get => _data;
            set
            {
                _data = value ?? new List<ChartDataPoint>();
                Invalidate();
            }
        }

        public StudioChartControl()
        {
            DoubleBuffered = true;
            BackColor = StudioColors.CardDark;
            Size = new Size(400, 240);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var path = RoundedPanel.GetRoundedPath(rect, 12);

            // Card background
            using (var bgBrush = new SolidBrush(StudioColors.CardDark))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (var borderPen = new Pen(StudioColors.BorderColor, 1))
            {
                e.Graphics.DrawPath(borderPen, path);
            }

            // Draw Title
            using var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, ChartTitle, titleFont, 
                new Rectangle(16, 12, Width - 32, 24), StudioColors.TextWhite, TextFormatFlags.Left);

            if (_data == null || _data.Count == 0)
            {
                TextRenderer.DrawText(e.Graphics, "Chưa có dữ liệu thống kê", StudioColors.RegularFont,
                    rect, StudioColors.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // Chart area
            int paddingLeft = 36;
            int paddingBottom = 34;
            int paddingTop = 48;
            int paddingRight = 20;

            int chartWidth = Width - paddingLeft - paddingRight;
            int chartHeight = Height - paddingTop - paddingBottom;
            if (chartWidth <= 0 || chartHeight <= 0) return;

            // Find Max Value
            double maxValue = 1;
            foreach (var item in _data)
            {
                if (item.Value > maxValue) maxValue = item.Value;
            }
            maxValue = Math.Ceiling(maxValue * 1.2); // chừa khoảng trên

            // Draw Grid Lines
            using var gridPen = new Pen(Color.FromArgb(45, 45, 65), 1) { DashStyle = DashStyle.Dash };
            for (int i = 0; i <= 4; i++)
            {
                int y = paddingTop + (int)(chartHeight * (i / 4.0));
                e.Graphics.DrawLine(gridPen, paddingLeft, y, Width - paddingRight, y);

                double valLabel = Math.Round(maxValue * (1.0 - i / 4.0), 1);
                TextRenderer.DrawText(e.Graphics, valLabel.ToString("0"), StudioColors.SmallFont,
                    new Rectangle(4, y - 8, paddingLeft - 8, 16), StudioColors.TextMuted, TextFormatFlags.Right);
            }

            // Draw Bars
            int count = _data.Count;
            int barSlotWidth = chartWidth / count;
            int barWidth = Math.Max(12, Math.Min(36, barSlotWidth - 14));

            for (int i = 0; i < count; i++)
            {
                var item = _data[i];
                int xCenter = paddingLeft + i * barSlotWidth + barSlotWidth / 2;
                int xBar = xCenter - barWidth / 2;

                int barH = (int)((item.Value / maxValue) * chartHeight);
                int yBar = paddingTop + chartHeight - barH;

                var barRect = new Rectangle(xBar, yBar, barWidth, barH);
                if (barH > 4)
                {
                    Color barCol = ColorTranslator.FromHtml(item.ColorHex);
                    Color barColEnd = Color.FromArgb(Math.Min(255, barCol.R + 40), Math.Min(255, barCol.G + 40), Math.Min(255, barCol.B + 40));

                    using var barPath = RoundedPanel.GetRoundedPath(barRect, 4);
                    using var brush = new LinearGradientBrush(barRect, barColEnd, barCol, LinearGradientMode.Vertical);
                    e.Graphics.FillPath(brush, barPath);

                    // Glow line on top of bar
                    using var topPen = new Pen(Color.FromArgb(200, 255, 255, 255), 1.5f);
                    e.Graphics.DrawLine(topPen, xBar + 2, yBar, xBar + barWidth - 2, yBar);
                }

                // Value Text on top of bar
                TextRenderer.DrawText(e.Graphics, item.Value.ToString("0"), StudioColors.BadgeFont,
                    new Rectangle(xBar - 10, yBar - 18, barWidth + 20, 16), StudioColors.TextWhite,
                    TextFormatFlags.HorizontalCenter);

                // Label on X-axis
                TextRenderer.DrawText(e.Graphics, item.Label, StudioColors.SmallFont,
                    new Rectangle(xBar - 15, paddingTop + chartHeight + 6, barWidth + 30, 20),
                    StudioColors.TextMuted, TextFormatFlags.HorizontalCenter);
            }
        }
    }
}
