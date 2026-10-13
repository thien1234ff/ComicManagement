using System.Drawing;

namespace ComicManagement.Controls
{
    public static class StudioColors
    {
        // Backgrounds
        public static readonly Color BackgroundDark = Color.FromArgb(24, 24, 37);      // #181825
        public static readonly Color SidebarDark = Color.FromArgb(30, 30, 46);         // #1E1E2E
        public static readonly Color CardDark = Color.FromArgb(37, 37, 56);            // #252538
        public static readonly Color CardHover = Color.FromArgb(46, 46, 68);           // #2E2E44
        public static readonly Color InputBackground = Color.FromArgb(30, 30, 46);     // #1E1E2E
        public static readonly Color BorderColor = Color.FromArgb(56, 56, 84);          // #383854

        // Accents Neon & Vibrancy
        public static readonly Color PrimaryPurple = Color.FromArgb(124, 58, 237);     // #7C3AED (Neon Purple)
        public static readonly Color PrimaryPurpleHover = Color.FromArgb(139, 92, 246);// #8B5CF6
        public static readonly Color SecondaryPink = Color.FromArgb(236, 72, 153);     // #EC4899 (Neon Pink)
        public static readonly Color CyanNeon = Color.FromArgb(6, 182, 212);          // #06B6D4 (Cyber Cyan)

        // Status Colors (3 Cấp độ Deadline trực quan)
        public static readonly Color SafeGreen = Color.FromArgb(16, 185, 129);         // #10B981 (> 2 ngày - Xanh lá an toàn)
        public static readonly Color WarningAmber = Color.FromArgb(245, 158, 11);      // #F59E0B (<= 48h - Vàng/Cam gấp)
        public static readonly Color DangerRed = Color.FromArgb(239, 68, 68);          // #EF4444 (Quá hạn - Đỏ rực)
        public static readonly Color InfoBlue = Color.FromArgb(59, 130, 246);          // #3B82F6 (Thông tin chung)

        // Texts
        public static readonly Color TextWhite = Color.FromArgb(248, 250, 252);        // #F8FAFC
        public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);        // #94A3B8
        public static readonly Color TextSecondary = Color.FromArgb(203, 213, 225);    // #CBD5E1

        // Fonts
        public static readonly Font HeaderFont = new Font("Segoe UI", 16, FontStyle.Bold);
        public static readonly Font TitleFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font RegularFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font SmallFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font BadgeFont = new Font("Segoe UI", 8f, FontStyle.Bold);
    }
}
