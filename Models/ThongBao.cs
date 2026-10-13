using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicManagement.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? NguoiNhanId { get; set; }

        [ForeignKey(nameof(NguoiNhanId))]
        public virtual ThanhVien? NguoiNhan { get; set; }

        [Required]
        [MaxLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LoaiThongBao { get; set; } = "General"; // TaskAssigned, WarningExpiring, UrgentOverdue, General

        [Required]
        [MaxLength(20)]
        public string MucDo { get; set; } = "Info"; // Info, Warning, Danger

        public bool DaDoc { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? CongDoanDeadlineId { get; set; }

        [ForeignKey(nameof(CongDoanDeadlineId))]
        public virtual CongDoanDeadline? CongDoanDeadline { get; set; }

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - NgayTao;
                if (diff.TotalMinutes < 1) return "Vừa xong";
                if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes} phút trước";
                if (diff.TotalDays < 1) return $"{(int)diff.TotalHours} giờ trước";
                return $"{(int)diff.TotalDays} ngày trước";
            }
        }
    }
}
