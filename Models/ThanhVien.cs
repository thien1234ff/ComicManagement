using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicManagement.Models
{
    [Table("ThanhVien")]
    public class ThanhVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string MatKhauHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SoDienThoai { get; set; }

        [MaxLength(50)]
        public string? DiscordTag { get; set; }

        [Required]
        [MaxLength(50)]
        public string VaiTroChuyenMon { get; set; } = StudioRoles.Translator;

        [Required]
        [MaxLength(20)]
        public string TrangThaiHoatDong { get; set; } = MemberStatuses.Active;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<CongDoanDeadline> CacCongDoan { get; set; } = new List<CongDoanDeadline>();
        public virtual ICollection<ThongBao> CacThongBao { get; set; } = new List<ThongBao>();

        [NotMapped]
        public bool IsAdmin => VaiTroChuyenMon.Equals(StudioRoles.LeaderAdmin, StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public int ActiveTasksCount => CacCongDoan?.Count(c => c.TrangThai != TaskStatuses.Completed) ?? 0;
    }
}
