using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ComicManagement.Controls
{
    public class RoundedPanel : Panel
    {
        public int BorderRadius { get; set; } = 10;
        public Color BorderColor { get; set; } = StudioColors.BorderColor;
        public int BorderSize { get; set; } = 1;
        public Color CustomBackgroundColor { get; set; } = StudioColors.CardDark;
        public bool UseGradient { get; set; } = false;
        public Color GradientEndColor { get; set; } = StudioColors.CardHover;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.ResizeRedraw | 
                     ControlStyles.UserPaint, true);
            UpdateStyles();

            DoubleBuffered = true;
            BackColor = StudioColors.CardDark;
            Padding = new Padding(10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Xóa nền cha
            Color parentBg = Parent != null ? Parent.BackColor : StudioColors.BackgroundDark;
            e.Graphics.Clear(parentBg);

            var rect = new Rectangle(BorderSize, BorderSize, Width - BorderSize * 2 - 1, Height - BorderSize * 2 - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = GetRoundedPath(rect, BorderRadius);

            // Fill background
            if (UseGradient)
            {
                using var brush = new LinearGradientBrush(rect, CustomBackgroundColor, GradientEndColor, 135f);
                e.Graphics.FillPath(brush, path);
            }
            else
            {
                using var brush = new SolidBrush(CustomBackgroundColor);
                e.Graphics.FillPath(brush, path);
            }

            // Draw border
            if (BorderSize > 0 && BorderColor != Color.Transparent)
            {
                using var pen = new Pen(BorderColor, BorderSize);
                e.Graphics.DrawPath(pen, path);
            }
        }

        public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            float r = Math.Min(radius, Math.Min(rect.Width / 2f, rect.Height / 2f));
            if (r <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
