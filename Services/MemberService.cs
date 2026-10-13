using System;
using System.Collections.Generic;
using System.Linq;
using ComicManagement.Data;
using ComicManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicManagement.Services
{
    public class MemberWorkloadDto
    {
        public ThanhVien Member { get; set; } = null!;
        public int TotalAssignedTasks { get; set; }
        public int ActiveTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CompletedTasks { get; set; }
        public string WorkloadLevel { get; set; } = "Bình thường"; // Rảnh, Vừa phải, Bận rộn, Quá tải
        public string WorkloadColorHex { get; set; } = "#10B981"; // Xanh lá, Cam, Đỏ
    }

    public class MemberService
    {
        public List<ThanhVien> GetAllMembers(string? keyword = null, string? role = null, string? status = null)
        {
            using var db = new ComicDbContext();
            var query = db.ThanhViens
                .Include(t => t.CacCongDoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(t => t.HoTen.ToLower().Contains(kw) ||
                                         t.TenDangNhap.ToLower().Contains(kw) ||
                                         t.Email.ToLower().Contains(kw) ||
                                         (t.DiscordTag != null && t.DiscordTag.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "Tất cả")
            {
                query = query.Where(t => t.VaiTroChuyenMon == role);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả")
            {
                query = query.Where(t => t.TrangThaiHoatDong == status);
            }

            return query.OrderBy(t => t.HoTen).ToList();
        }

        public List<MemberWorkloadDto> GetMembersWithWorkload()
        {
            using var db = new ComicDbContext();
            var members = db.ThanhViens
                .Include(t => t.CacCongDoan)
                .ToList();

            var result = new List<MemberWorkloadDto>();

            foreach (var m in members)
            {
                var tasks = m.CacCongDoan.ToList();
                int total = tasks.Count;
                int active = tasks.Count(t => t.TrangThai != TaskStatuses.Completed);
                int overdue = tasks.Count(t => t.Urgency == DeadlineUrgency.Overdue);
                int completed = tasks.Count(t => t.TrangThai == TaskStatuses.Completed);

                string level = "Thảnh thơi";
                string colorHex = "#10B981"; // Green

                if (overdue > 0 || active >= 5)
                {
                    level = "Cảnh báo quá tải";
                    colorHex = "#EF4444"; // Red
                }
                else if (active >= 3)
                {
                    level = "Khá bận rộn";
                    colorHex = "#F59E0B"; // Orange
                }
                else if (active >= 1)
                {
                    level = "Vừa phải";
                    colorHex = "#06B6D4"; // Cyan
                }

                result.Add(new MemberWorkloadDto
                {
                    Member = m,
                    TotalAssignedTasks = total,
                    ActiveTasks = active,
                    OverdueTasks = overdue,
                    CompletedTasks = completed,
                    WorkloadLevel = level,
                    WorkloadColorHex = colorHex
                });
            }

            return result.OrderByDescending(w => w.ActiveTasks).ToList();
        }

        public (bool Success, string Message, ThanhVien? Member) AddMember(ThanhVien member, string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(member.HoTen) || string.IsNullOrWhiteSpace(member.TenDangNhap) || string.IsNullOrWhiteSpace(member.Email))
            {
                return (false, "Vui lòng nhập đầy đủ Họ tên, Tên đăng nhập và Email!", null);
            }

            using var db = new ComicDbContext();
            if (db.ThanhViens.Any(t => t.TenDangNhap == member.TenDangNhap))
            {
                return (false, "Tên đăng nhập đã tồn tại!", null);
            }

            if (db.ThanhViens.Any(t => t.Email == member.Email))
            {
                return (false, "Email đã được sử dụng!", null);
            }

            member.MatKhauHash = AuthService.HashPassword(string.IsNullOrWhiteSpace(plainPassword) ? "123456" : plainPassword);
            member.NgayTao = DateTime.Now;

            db.ThanhViens.Add(member);
            db.SaveChanges();

            return (true, "Thêm thành viên thành công!", member);
        }

        public (bool Success, string Message) UpdateMember(ThanhVien member, string? newPassword = null)
        {
            using var db = new ComicDbContext();
            var existing = db.ThanhViens.Find(member.Id);
            if (existing == null) return (false, "Không tìm thấy thành viên!");

            existing.HoTen = member.HoTen;
            existing.Email = member.Email;
            existing.SoDienThoai = member.SoDienThoai;
            existing.DiscordTag = member.DiscordTag;
            existing.VaiTroChuyenMon = member.VaiTroChuyenMon;
            existing.TrangThaiHoatDong = member.TrangThaiHoatDong;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                existing.MatKhauHash = AuthService.HashPassword(newPassword);
            }

            db.SaveChanges();
            return (true, "Cập nhật thành viên thành công!");
        }

        public (bool Success, string Message) UpdateMemberStatus(int id, string newStatus)
        {
            using var db = new ComicDbContext();
            var existing = db.ThanhViens.Find(id);
            if (existing == null) return (false, "Không tìm thấy thành viên!");

            existing.TrangThaiHoatDong = newStatus;
            db.SaveChanges();
            return (true, "Đã cập nhật trạng thái!");
        }

        public (bool Success, string Message) DeleteMember(int id)
        {
            using var db = new ComicDbContext();
            var existing = db.ThanhViens.Find(id);
            if (existing == null) return (false, "Không tìm thấy thành viên!");

            if (existing.IsAdmin && db.ThanhViens.Count(u => u.VaiTroChuyenMon == StudioRoles.LeaderAdmin) <= 1)
            {
                return (false, "Không thể xóa Admin/Leader duy nhất của hệ thống!");
            }

            db.ThanhViens.Remove(existing);
            db.SaveChanges();
            return (true, "Đã xóa thành viên thành công!");
        }
    }
}
