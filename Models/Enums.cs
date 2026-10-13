using System;

namespace ComicManagement.Models
{
    public static class StudioRoles
    {
        public const string LeaderAdmin = "Leader/Admin";
        public const string Translator = "Translator";
        public const string CleanerRedraw = "Cleaner/Redraw";
        public const string Typesetter = "Typesetter";
        public const string QC = "QC";

        public static readonly string[] AllRoles = new[]
        {
            LeaderAdmin,
            Translator,
            CleanerRedraw,
            Typesetter,
            QC
        };
    }

    public static class PipelineStages
    {
        public const string RawScan = "Raw/Scan";
        public const string Translate = "Dịch (Trans)";
        public const string CleanRedraw = "Xóa thoại & Vẽ lại (Clean)";
        public const string Typeset = "Điền thoại (Typeset)";
        public const string QualityCheck = "Duyệt chất lượng (QC)";
        public const string Completed = "Hoàn tất";

        public static readonly string[] AllStages = new[]
        {
            Translate,
            CleanRedraw,
            Typeset,
            QualityCheck
        };
    }

    public static class TaskStatuses
    {
        public const string NotStarted = "Chưa nhận";
        public const string InProgress = "Đang làm";
        public const string PendingApproval = "Chờ duyệt";
        public const string Completed = "Hoàn thành";

        public static readonly string[] AllStatuses = new[]
        {
            NotStarted,
            InProgress,
            PendingApproval,
            Completed
        };
    }

    public enum DeadlineUrgency
    {
        Safe,       // > 48h (Xanh lá #10B981)
        Warning,    // <= 48h (Vàng/Cam #F59E0B)
        Overdue,    // Đã quá hạn (Đỏ rực #EF4444)
        Completed   // Đã xong (Tím neon hoặc Xám sáng)
    }

    public static class MemberStatuses
    {
        public const string Active = "Active";
        public const string Busy = "Busy";
        public const string OnLeave = "On Leave";

        public static readonly string[] All = new[] { Active, Busy, OnLeave };
    }

    public static class ComicStatuses
    {
        public const string InProgress = "Đang tiến hành";
        public const string OnHold = "Tạm ngưng";
        public const string Completed = "Đã hoàn thành";

        public static readonly string[] All = new[] { InProgress, OnHold, Completed };
    }
}
