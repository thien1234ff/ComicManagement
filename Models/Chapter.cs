using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ComicManagement.Models
{
    [Table("Chapter")]
    public class Chapter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TruyenId { get; set; }

        [ForeignKey(nameof(TruyenId))]
        public virtual Truyen? Truyen { get; set; }

        [Required]
        public double SoChapter { get; set; }

        [MaxLength(200)]
        public string? TieuDe { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrangThai { get; set; } = "Đang biên tập"; // 'Chưa bắt đầu', 'Đang biên tập', 'Đã hoàn tất', 'Đã xuất bản'

        [MaxLength(500)]
        public string? FolderPsd { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public DateTime? NgayXuatBan { get; set; }

        // Navigation properties
        public virtual ICollection<CongDoanDeadline> CacCongDoan { get; set; } = new List<CongDoanDeadline>();

        [NotMapped]
        public string DisplayName => $"Chap {SoChapter}{(string.IsNullOrWhiteSpace(TieuDe) ? "" : ": " + TieuDe)}";

        [NotMapped]
        public int ProgressPercent
        {
            get
            {
                if (CacCongDoan == null || CacCongDoan.Count == 0) return 0;
                int completed = CacCongDoan.Count(c => c.TrangThai == TaskStatuses.Completed);
                return (int)Math.Round((double)completed / CacCongDoan.Count * 100);
            }
        }
    }
}
