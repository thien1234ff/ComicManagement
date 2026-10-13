using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ComicManagement.Models;

namespace ComicManagement.Controls
{
    public class CoverCardControl : UserControl
    {
        private Truyen? _comic;
        private bool _isHovered = false;

        public event EventHandler<Truyen>? ComicClicked;

        public Truyen? Comic
        {
            get => _comic;
            set
            {
                _comic = value;
                Invalidate();
            }
        }

        public CoverCardControl()
        {
            DoubleBuffered = true;
            Size = new Size(240, 336);
            Margin = new Padding(0, 0, 16, 16);
            Cursor = Cursors.Hand;
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
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_comic != null)
            {
                ComicClicked?.Invoke(this, _comic);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Parent?.BackColor ?? StudioColors.BackgroundDark);

            var cardRect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var cardPath = RoundedPanel.GetRoundedPath(cardRect, 10);

            // Background
            Color bgColor = _isHovered ? StudioColors.CardHover : StudioColors.CardDark;
            using (var bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(bgBrush, cardPath);
            }

            // Border
            Color borderColor = _isHovered ? StudioColors.PrimaryPurple : StudioColors.BorderColor;
            using (var borderPen = new Pen(borderColor, _isHovered ? 1.5f : 1f))
            {
                e.Graphics.DrawPath(borderPen, cardPath);
            }

            if (_comic == null) return;

            // 1. Cover Graphic Box
            var coverRect = new Rectangle(8, 8, Width - 16, 140);
            using (var coverPath = RoundedPanel.GetRoundedPath(coverRect, 8))
            {
                Color c1 = (_comic.LoaiTruyen == "Manga") ? Color.FromArgb(60, 40, 100) : 
                           (_comic.LoaiTruyen == "Manhwa") ? Color.FromArgb(30, 60, 110) : Color.FromArgb(100, 35, 45);
                Color c2 = Color.FromArgb(20, 20, 32);

                using var gradBrush = new LinearGradientBrush(coverRect, c1, c2, LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillPath(gradBrush, coverPath);

                // Loại truyện (text thuần túy góc trái)
                using var tagFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, _comic.LoaiTruyen.ToUpper(), tagFont, 
                    new Rectangle(coverRect.X + 8, coverRect.Y + 8, 80, 22), 
                    Color.FromArgb(220, 255, 255, 255));

                // Status Badge ngắn gọn, không bị cắt chữ góc phải
                string statusText = _comic.TrangThai switch
                {
                    ComicStatuses.InProgress => "Đang làm",
                    ComicStatuses.OnHold => "Tạm dừng",
                    ComicStatuses.Completed => "Hoàn thành",
                    _ => _comic.TrangThai
                };
                Color statusBg = (_comic.TrangThai == ComicStatuses.InProgress) ? StudioColors.CyanNeon :
                                 (_comic.TrangThai == ComicStatuses.Completed) ? StudioColors.SafeGreen : StudioColors.WarningAmber;
                
                var statusBadgeRect = new Rectangle(coverRect.Right - 84, coverRect.Y + 8, 76, 22);
                using var badgePath = RoundedPanel.GetRoundedPath(statusBadgeRect, 4);
                using var badgeBrush = new SolidBrush(Color.FromArgb(220, statusBg));
                e.Graphics.FillPath(badgeBrush, badgePath);

                using var badgeFont = new Font("Segoe UI", 8f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, statusText, badgeFont, statusBadgeRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 2. Comic Title (Tên truyện - chiều cao 40px thoáng đãng)
            int currentY = 156;
            var titleRect = new Rectangle(12, currentY, Width - 24, 40);
            using var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, _comic.TenTruyen, titleFont, titleRect, StudioColors.TextWhite,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);

            // 3. Tác giả chính & Thể loại tiêu biểu (gọn gàng, đầy đủ, không bị cắt '...')
            currentY += 44;
            var authorRect = new Rectangle(12, currentY, Width - 24, 18);
            using var subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);

            string authorText = _comic.TacGia;
            int parenIdx = authorText.IndexOf('(');
            if (parenIdx > 0) authorText = authorText.Substring(0, parenIdx).Trim();
            int commaIdx = authorText.IndexOf(',');
            if (commaIdx > 0) authorText = authorText.Substring(0, commaIdx).Trim();
            string authorDisplay = $"Tác giả: {authorText}";
            TextRenderer.DrawText(e.Graphics, authorDisplay, subFont, authorRect, StudioColors.TextMuted,
                TextFormatFlags.Left);

            currentY += 20;
            var genreRect = new Rectangle(12, currentY, Width - 24, 18);
            string genreText = _comic.TheLoai;
            var genres = genreText.Split(new[] { ',', '•' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string genreDisplay = genres.Length >= 2 
                ? $"Thể loại: {genres[0]}, {genres[1]}" 
                : $"Thể loại: {genreText}";
            TextRenderer.DrawText(e.Graphics, genreDisplay, subFont, genreRect, StudioColors.TextSecondary,
                TextFormatFlags.Left);

            // 4. Progress Bar (Tách biệt hoàn toàn, không bao giờ đè lên chữ)
            currentY += 24;
            double progress = _comic.CompletionPercentage;
            var progressTrackRect = new Rectangle(12, currentY, Width - 24, 5);
            using (var trackPath = RoundedPanel.GetRoundedPath(progressTrackRect, 3))
            using (var trackBrush = new SolidBrush(Color.FromArgb(45, 45, 65)))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            int fillWidth = (int)((Width - 24) * (progress / 100.0));
            if (fillWidth > 4)
            {
                var progressFillRect = new Rectangle(12, currentY, fillWidth, 5);
                using var fillPath = RoundedPanel.GetRoundedPath(progressFillRect, 3);
                using var fillBrush = new SolidBrush(StudioColors.PrimaryPurple);
                e.Graphics.FillPath(fillBrush, fillPath);
            }

            // 5. Chapters count & Progress text
            currentY += 11;
            var infoRect = new Rectangle(12, currentY, Width - 24, 18);
            string chapInfo = $"{_comic.PublishedChapters}/{_comic.TotalChapters} chap";
            TextRenderer.DrawText(e.Graphics, chapInfo, StudioColors.SmallFont, infoRect, StudioColors.TextMuted, TextFormatFlags.Left);

            string pctText = $"{progress}%";
            TextRenderer.DrawText(e.Graphics, pctText, StudioColors.BadgeFont, infoRect, StudioColors.CyanNeon, TextFormatFlags.Right);

            // 6. Action Button ở đáy
            currentY += 24;
            var btnRect = new Rectangle(12, currentY, Width - 24, 30);
            using (var btnPath = RoundedPanel.GetRoundedPath(btnRect, 5))
            using (var btnBrush = new SolidBrush(_isHovered ? StudioColors.PrimaryPurple : Color.FromArgb(45, 45, 68)))
            {
                e.Graphics.FillPath(btnBrush, btnPath);
            }
            using var btnFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, "Quản Lý Chapter", btnFont, btnRect, StudioColors.TextWhite,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
