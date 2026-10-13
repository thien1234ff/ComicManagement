using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ComicManagement.Models
{
    [Table("Truyen")]
    public class Truyen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TenTruyen { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TacGia { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TheLoai { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LoaiTruyen { get; set; } = "Manga"; // Manga, Manhwa, Comic

        [Required]
        [MaxLength(50)]
        public string TrangThai { get; set; } = ComicStatuses.InProgress;

        [MaxLength(500)]
        public string? FolderRawPsd { get; set; }

        [MaxLength(500)]
        public string? HinhAnhBia { get; set; }

        public string? MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        [NotMapped]
        public int TotalChapters => Chapters?.Count ?? 0;

        [NotMapped]
        public int PublishedChapters => Chapters?.Count(c => c.TrangThai == "Đã xuất bản" || c.TrangThai == "Đã hoàn tất") ?? 0;

        [NotMapped]
        public double CompletionPercentage
        {
            get
            {
                if (Chapters == null || Chapters.Count == 0) return 0;
                double done = Chapters.Count(c => c.TrangThai == "Đã xuất bản" || c.TrangThai == "Đã hoàn tất");
                return Math.Round((done / Chapters.Count) * 100, 1);
            }
        }
    }
}
