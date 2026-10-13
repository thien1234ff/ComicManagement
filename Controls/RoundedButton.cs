using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ComicManagement.Controls
{
    public class RoundedButton : Button
    {
        public int BorderRadius { get; set; } = 8;
        public Color NormalColor { get; set; } = StudioColors.PrimaryPurple;
        public Color HoverColor { get; set; } = StudioColors.PrimaryPurpleHover;
        public Color PressedColor { get; set; } = Color.FromArgb(109, 40, 217);
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderSize { get; set; } = 0;

        private bool _isHovered = false;
        private bool _isPressed = false;

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.ResizeRedraw | 
                     ControlStyles.UserPaint, true);
            UpdateStyles();

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            ForeColor = StudioColors.TextWhite;
            Cursor = Cursors.Hand;
            Size = new Size(130, 40);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Xóa sạch nền cũ bằng màu nền cha để triệt tiêu 100% hiện tượng text ghosting
            Color parentBg = Parent != null ? Parent.BackColor : StudioColors.SidebarDark;
            pevent.Graphics.Clear(parentBg);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = RoundedPanel.GetRoundedPath(rect, BorderRadius);

            Color currentBg = _isPressed ? PressedColor : (_isHovered ? HoverColor : NormalColor);
            if (!Enabled)
            {
                currentBg = Color.FromArgb(60, 60, 80);
            }

            using (var brush = new SolidBrush(currentBg))
            {
                pevent.Graphics.FillPath(brush, path);
            }

            if (BorderSize > 0 && BorderColor != Color.Transparent)
            {
                using var pen = new Pen(BorderColor, BorderSize);
                pevent.Graphics.DrawPath(pen, path);
            }

            // Draw Text rõ ràng
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            Color textCol = Enabled ? ForeColor : StudioColors.TextMuted;
            TextRenderer.DrawText(pevent.Graphics, Text, Font, rect, textCol, flags);
        }
    }
}
