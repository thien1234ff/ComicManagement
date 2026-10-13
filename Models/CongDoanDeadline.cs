using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicManagement.Models
{
    [Table("CongDoanDeadline")]
    public class CongDoanDeadline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ChapterId { get; set; }

        [ForeignKey(nameof(ChapterId))]
        public virtual Chapter? Chapter { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenCongDoan { get; set; } = string.Empty;

        public int ThuTuPipeline { get; set; } = 1;

        public int? ThanhVienId { get; set; }

        [ForeignKey(nameof(ThanhVienId))]
        public virtual ThanhVien? ThanhVien { get; set; }

        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required]
        public DateTime HanChot { get; set; }

        public DateTime? NgayHoanThanh { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrangThai { get; set; } = TaskStatuses.InProgress;

        [MaxLength(500)]
        public string? LinkNopSanPham { get; set; }

        [MaxLength(500)]
        public string? GhiChu { get; set; }

        // --- Helper / Computed properties ---

        [NotMapped]
        public DeadlineUrgency Urgency
        {
            get
            {
                if (TrangThai == TaskStatuses.Completed)
                    return DeadlineUrgency.Completed;

                var diff = HanChot - DateTime.Now;
                if (diff.TotalSeconds < 0)
                    return DeadlineUrgency.Overdue;
                else if (diff.TotalHours <= 48)
                    return DeadlineUrgency.Warning;
                else
                    return DeadlineUrgency.Safe;
            }
        }

        [NotMapped]
        public string UrgencyDescription
        {
            get
            {
                if (TrangThai == TaskStatuses.Completed)
                    return "Hoàn tất";

                var diff = HanChot - DateTime.Now;
                if (diff.TotalSeconds < 0)
                {
                    int overdueDays = Math.Max(1, (int)Math.Abs(diff.TotalDays));
                    return $"Trễ {overdueDays} ngày";
                }
                else if (diff.TotalHours < 24)
                {
                    int hours = Math.Max(1, (int)diff.TotalHours);
                    return $"Còn {hours}h";
                }
                else if (diff.TotalHours <= 48)
                {
                    return "Còn 2 ngày";
                }
                else
                {
                    int days = (int)Math.Ceiling(diff.TotalDays);
                    return $"Còn {days} ngày";
                }
            }
        }

        [NotMapped]
        public string MemberDisplayName => ThanhVien?.HoTen ?? "Chưa phân công";

        [NotMapped]
        public string ComicAndChapterName
        {
            get
            {
                if (Chapter?.Truyen != null)
                {
                    string title = Chapter.Truyen.TenTruyen;
                    int colonIdx = title.IndexOf(':');
                    if (colonIdx > 0 && colonIdx <= 18)
                    {
                        title = title.Substring(0, colonIdx).Trim();
                    }
                    else if (title.Length > 16)
                    {
                        title = title.Substring(0, 16).Trim();
                    }
                    return $"{title} • Chap {Chapter.SoChapter}";
                }
                if (Chapter != null)
                    return $"Chap {Chapter.SoChapter}";
                return "Nhiệm vụ";
            }
        }
    }
}
