using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ComicManagement.Controls
{
    public class StudioProgressBar : Control
    {
        private int _value = 0;
        public int Maximum { get; set; } = 100;
        public int Minimum { get; set; } = 0;

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(Minimum, Math.Min(Maximum, value));
                Invalidate();
            }
        }

        public Color StartColor { get; set; } = StudioColors.PrimaryPurple;
        public Color EndColor { get; set; } = StudioColors.SecondaryPink;
        public Color ChannelColor { get; set; } = Color.FromArgb(40, 40, 60);
        public bool ShowPercentageText { get; set; } = true;
        public int BorderRadius { get; set; } = 6;

        public StudioProgressBar()
        {
            DoubleBuffered = true;
            Size = new Size(160, 16);
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            ForeColor = StudioColors.TextWhite;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            // Draw track / channel
            using (var trackPath = RoundedPanel.GetRoundedPath(rect, BorderRadius))
            using (var trackBrush = new SolidBrush(ChannelColor))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            // Draw progress bar
            float percent = (Maximum > Minimum) ? (float)(Value - Minimum) / (Maximum - Minimum) : 0f;
            int progressWidth = (int)(rect.Width * percent);

            if (progressWidth > 4)
            {
                var progressRect = new Rectangle(0, 0, progressWidth, rect.Height);
                using var progressPath = RoundedPanel.GetRoundedPath(progressRect, BorderRadius);
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, Math.Max(rect.Width, 10), rect.Height),
                    StartColor,
                    EndColor,
                    LinearGradientMode.Horizontal);

                e.Graphics.FillPath(brush, progressPath);
            }

            // Show text
            if (ShowPercentageText)
            {
                string text = $"{Value}%";
                TextRenderer.DrawText(e.Graphics, text, Font, rect, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
