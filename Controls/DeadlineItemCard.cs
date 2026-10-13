using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ComicManagement.Models;

namespace ComicManagement.Controls
{
    public class DeadlineItemCard : UserControl
    {
        private CongDoanDeadline? _deadline;
        private bool _isHovered = false;

        public event EventHandler<CongDoanDeadline>? QuickActionClicked;
        public event EventHandler<CongDoanDeadline>? EditClicked;

        public CongDoanDeadline? Deadline
        {
            get => _deadline;
            set
            {
                _deadline = value;
                Invalidate();
            }
        }

        private bool _isHoverQuick = false;
        private bool _isHoverEdit = false;

        public DeadlineItemCard()
        {
            DoubleBuffered = true;
            Size = new Size(250, 132);
            Margin = new Padding(0, 0, 10, 12);
            Cursor = Cursors.Default;
            BackColor = Color.Transparent;
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
            _isHoverQuick = false;
            _isHoverEdit = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var btnQuickRect = new Rectangle(12, Height - 36, 146, 26);
            var btnEditRect = new Rectangle(166, Height - 36, Width - 178, 26);

            bool hoverQuick = btnQuickRect.Contains(e.Location);
            bool hoverEdit = btnEditRect.Contains(e.Location);

            if (hoverQuick != _isHoverQuick || hoverEdit != _isHoverEdit)
            {
                _isHoverQuick = hoverQuick;
                _isHoverEdit = hoverEdit;
                Invalidate();
            }

            Cursor = (hoverQuick || hoverEdit) ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_deadline == null) return;

            // Nút "Cập Nhật"
            var btnQuickRect = new Rectangle(12, Height - 36, 146, 26);
            if (btnQuickRect.Contains(e.Location))
            {
                QuickActionClicked?.Invoke(this, _deadline);
                return;
            }

            // Nút "Sửa"
            var btnEditRect = new Rectangle(166, Height - 36, Width - 178, 26);
            if (btnEditRect.Contains(e.Location))
            {
                EditClicked?.Invoke(this, _deadline);
                return;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_deadline == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var cardRect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var cardPath = RoundedPanel.GetRoundedPath(cardRect, 8);

            // Nền
            Color bg = _isHovered ? Color.FromArgb(42, 42, 62) : StudioColors.CardDark;
            using (var brush = new SolidBrush(bg))
            {
                e.Graphics.FillPath(brush, cardPath);
            }

            // Màu mức độ hạn chót
            Color urgencyColor = _deadline.Urgency switch
            {
                DeadlineUrgency.Overdue => StudioColors.DangerRed,
                DeadlineUrgency.Warning => StudioColors.WarningAmber,
                _ => StudioColors.SafeGreen
            };

            // Viền
            Color border = _isHovered ? urgencyColor : StudioColors.BorderColor;
            using (var pen = new Pen(border, 1))
            {
                e.Graphics.DrawPath(pen, cardPath);
            }

            // Vạch màu trạng thái bên trái
            var stripeRect = new Rectangle(1, 3, 4, Height - 6);
            using (var stripeBrush = new SolidBrush(urgencyColor))
            {
                e.Graphics.FillRectangle(stripeBrush, stripeRect);
            }

            // 1. Tên truyện & Chap (Góc trái) + Badge Hạn chót (Góc phải)
            var comicRect = new Rectangle(12, 10, Width - 92, 20);
            using var comicFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, _deadline.ComicAndChapterName, comicFont, comicRect, StudioColors.CyanNeon,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            // Badge Trạng thái Hạn (Pill Shape gọn gàng)
            string badgeText = _deadline.UrgencyDescription;
            var badgeRect = new Rectangle(Width - 78, 8, 66, 20);
            using (var badgePath = RoundedPanel.GetRoundedPath(badgeRect, 6))
            using (var badgeBrush = new SolidBrush(Color.FromArgb(40, urgencyColor)))
            using (var badgePen = new Pen(urgencyColor, 1))
            {
                e.Graphics.FillPath(badgeBrush, badgePath);
                e.Graphics.DrawPath(badgePen, badgePath);
            }
            using var bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, badgeText, bFont, badgeRect, urgencyColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // 2. Tên Công Đoạn (Headline to rõ ràng)
            var stageRect = new Rectangle(12, 34, Width - 24, 22);
            using var stageFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, _deadline.TenCongDoan, stageFont, stageRect, StudioColors.TextWhite,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            // 3. Phụ trách & Hạn chót (1 dòng gọn gàng, thoáng đãng)
            var infoRect = new Rectangle(12, 58, Width - 24, 18);
            string infoText = $"{_deadline.MemberDisplayName}  •  Hạn: {_deadline.HanChot:dd/MM}";
            TextRenderer.DrawText(e.Graphics, infoText, StudioColors.SmallFont, infoRect, StudioColors.TextSecondary,
                TextFormatFlags.Left);

            // 4. Nút bấm đáy thẻ (Cực kỳ thanh lịch, chữ ngắn gọn không bị tràn)
            // Nút chính: Cập Nhật
            var btnQuickRect = new Rectangle(12, Height - 36, 146, 26);
            Color quickBg = _isHoverQuick ? StudioColors.PrimaryPurpleHover : Color.FromArgb(58, 48, 85);
            using (var btnPath = RoundedPanel.GetRoundedPath(btnQuickRect, 4))
            using (var btnBrush = new SolidBrush(quickBg))
            {
                e.Graphics.FillPath(btnBrush, btnPath);
            }
            using var btnFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, "Cập Nhật", btnFont, btnQuickRect, StudioColors.TextWhite,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Nút phụ: Sửa
            var btnEditRect = new Rectangle(166, Height - 36, Width - 178, 26);
            Color editBg = _isHoverEdit ? Color.FromArgb(55, 55, 78) : Color.FromArgb(42, 42, 60);
            using (var btnPath = RoundedPanel.GetRoundedPath(btnEditRect, 4))
            using (var btnBrush = new SolidBrush(editBg))
            {
                e.Graphics.FillPath(btnBrush, btnPath);
            }
            TextRenderer.DrawText(e.Graphics, "Sửa", btnFont, btnEditRect, StudioColors.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
