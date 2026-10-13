using System;
using System.Collections.Generic;
using System.Linq;
using ComicManagement.Data;
using ComicManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicManagement.Services
{
    public class DeadlineService
    {
        public List<CongDoanDeadline> GetDeadlines(
            int? truyenId = null,
            int? thanhVienId = null,
            string? stageName = null,
            string? taskStatus = null,
            DeadlineUrgency? urgencyFilter = null,
            string? keyword = null)
        {
            using var db = new ComicDbContext();
            var query = db.CongDoanDeadlines
                .Include(cd => cd.ThanhVien)
                .Include(cd => cd.Chapter)
                    .ThenInclude(c => c!.Truyen)
                .AsQueryable();

            if (truyenId.HasValue && truyenId.Value > 0)
            {
                query = query.Where(cd => cd.Chapter != null && cd.Chapter.TruyenId == truyenId.Value);
            }

            if (thanhVienId.HasValue && thanhVienId.Value > 0)
            {
                query = query.Where(cd => cd.ThanhVienId == thanhVienId.Value);
            }

            if (!string.IsNullOrWhiteSpace(stageName) && stageName != "Tất cả")
            {
                query = query.Where(cd => cd.TenCongDoan == stageName);
            }

            if (!string.IsNullOrWhiteSpace(taskStatus) && taskStatus != "Tất cả")
            {
                query = query.Where(cd => cd.TrangThai == taskStatus);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(cd =>
                    (cd.Chapter != null && cd.Chapter.Truyen != null && cd.Chapter.Truyen.TenTruyen.ToLower().Contains(kw)) ||
                    (cd.ThanhVien != null && cd.ThanhVien.HoTen.ToLower().Contains(kw)) ||
                    cd.TenCongDoan.ToLower().Contains(kw) ||
                    (cd.GhiChu != null && cd.GhiChu.ToLower().Contains(kw)));
            }

            var list = query.OrderBy(cd => cd.HanChot).ToList();

            if (urgencyFilter.HasValue)
            {
                list = list.Where(cd => cd.Urgency == urgencyFilter.Value).ToList();
            }

            return list;
        }

        public (bool Success, string Message, CongDoanDeadline? Task) AddDeadline(CongDoanDeadline deadline)
        {
            if (deadline.ChapterId <= 0)
            {
                return (false, "Vui lòng chọn Chapter!", null);
            }

            if (string.IsNullOrWhiteSpace(deadline.TenCongDoan))
            {
                return (false, "Vui lòng nhập tên công đoạn!", null);
            }

            using var db = new ComicDbContext();
            db.CongDoanDeadlines.Add(deadline);
            db.SaveChanges();

            // Nếu có phân công thành viên, tự động tạo thông báo
            if (deadline.ThanhVienId.HasValue)
            {
                var chapter = db.Chapters.Include(c => c.Truyen).FirstOrDefault(c => c.Id == deadline.ChapterId);
                string comicTitle = chapter?.Truyen?.TenTruyen ?? "Truyện";
                string chapNum = chapter?.SoChapter.ToString() ?? "";

                var notif = new ThongBao
                {
                    NguoiNhanId = deadline.ThanhVienId.Value,
                    TieuDe = $"[DEADLINE MỚI] {deadline.TenCongDoan}: {comicTitle} Chap {chapNum}",
                    NoiDung = $"Bạn vừa được phân công công đoạn '{deadline.TenCongDoan}'. Hạn chót: {deadline.HanChot:dd/MM/yyyy HH:mm}.",
                    LoaiThongBao = "TaskAssigned",
                    MucDo = "Info",
                    CongDoanDeadlineId = deadline.Id,
                    NgayTao = DateTime.Now
                };
                db.ThongBaos.Add(notif);
                db.SaveChanges();
            }

            return (true, "Tạo deadline thành công!", deadline);
        }

        public (bool Success, string Message) UpdateDeadline(CongDoanDeadline deadline)
        {
            using var db = new ComicDbContext();
            var existing = db.CongDoanDeadlines.Find(deadline.Id);
            if (existing == null) return (false, "Không tìm thấy công đoạn!");

            int? oldMemberId = existing.ThanhVienId;

            existing.TenCongDoan = deadline.TenCongDoan;
            existing.ThuTuPipeline = deadline.ThuTuPipeline;
            existing.ThanhVienId = deadline.ThanhVienId;
            existing.NgayBatDau = deadline.NgayBatDau;
            existing.HanChot = deadline.HanChot;
            existing.TrangThai = deadline.TrangThai;
            existing.LinkNopSanPham = deadline.LinkNopSanPham;
            existing.GhiChu = deadline.GhiChu;

            if (deadline.TrangThai == TaskStatuses.Completed && existing.NgayHoanThanh == null)
            {
                existing.NgayHoanThanh = DateTime.Now;
            }

            db.SaveChanges();

            // Nếu mới đổi người phụ trách, gửi thông báo
            if (deadline.ThanhVienId.HasValue && deadline.ThanhVienId != oldMemberId)
            {
                var notif = new ThongBao
                {
                    NguoiNhanId = deadline.ThanhVienId.Value,
                    TieuDe = $"[PHÂN CÔNG MỚI] Công đoạn: {deadline.TenCongDoan}",
                    NoiDung = $"Bạn vừa được chuyển giao phụ trách công đoạn '{deadline.TenCongDoan}'. Hạn chót: {deadline.HanChot:dd/MM/yyyy HH:mm}.",
                    LoaiThongBao = "TaskAssigned",
                    MucDo = "Info",
                    CongDoanDeadlineId = existing.Id,
                    NgayTao = DateTime.Now
                };
                db.ThongBaos.Add(notif);
                db.SaveChanges();
            }

            return (true, "Cập nhật deadline thành công!");
        }

        public (bool Success, string Message) QuickUpdateProgress(int deadlineId, string newStatus, string? linkNop = null)
        {
            using var db = new ComicDbContext();
            var item = db.CongDoanDeadlines.Include(cd => cd.Chapter).FirstOrDefault(cd => cd.Id == deadlineId);
            if (item == null) return (false, "Không tìm thấy công đoạn!");

            item.TrangThai = newStatus;
            if (!string.IsNullOrWhiteSpace(linkNop))
            {
                item.LinkNopSanPham = linkNop;
            }

            if (newStatus == TaskStatuses.Completed)
            {
                item.NgayHoanThanh = DateTime.Now;
            }

            db.SaveChanges();

            // Tự động kiểm tra nếu tất cả công đoạn của chapter đã xong thì cập nhật Chapter -> Đã hoàn tất
            if (item.Chapter != null)
            {
                bool allDone = db.CongDoanDeadlines
                    .Where(cd => cd.ChapterId == item.ChapterId)
                    .All(cd => cd.TrangThai == TaskStatuses.Completed);

                if (allDone)
                {
                    item.Chapter.TrangThai = "Đã hoàn tất";
                    db.SaveChanges();
                }
            }

            return (true, $"Đã cập nhật trạng thái thành '{newStatus}'!");
        }

        public (bool Success, string Message) DeleteDeadline(int id)
        {
            using var db = new ComicDbContext();
            var item = db.CongDoanDeadlines.Find(id);
            if (item == null) return (false, "Không tìm thấy công đoạn!");

            db.CongDoanDeadlines.Remove(item);
            db.SaveChanges();
            return (true, "Đã xóa công đoạn deadline!");
        }
    }
}
