using System;
using System.Collections.Generic;
using System.Linq;
using ComicManagement.Data;
using ComicManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicManagement.Services
{
    public class NotificationService
    {
        public void AutoScanAndGenerateDeadlineAlerts()
        {
            using var db = new ComicDbContext();
            var activeDeadlines = db.CongDoanDeadlines
                .Include(cd => cd.Chapter)
                    .ThenInclude(c => c!.Truyen)
                .Include(cd => cd.ThanhVien)
                .Where(cd => cd.TrangThai != TaskStatuses.Completed)
                .ToList();

            var now = DateTime.Now;

            foreach (var task in activeDeadlines)
            {
                if (!task.ThanhVienId.HasValue) continue;

                var diff = task.HanChot - now;
                string comicName = task.Chapter?.Truyen?.TenTruyen ?? "Truyện";
                string chapNum = task.Chapter?.SoChapter.ToString() ?? "";

                if (diff.TotalSeconds < 0)
                {
                    // Quá hạn -> UrgentOverdue
                    bool alreadySentOverdue = db.ThongBaos.Any(t => 
                        t.CongDoanDeadlineId == task.Id && 
                        t.LoaiThongBao == "UrgentOverdue" && 
                        t.NgayTao >= now.AddHours(-24));

                    if (!alreadySentOverdue)
                    {
                        int overdueDays = Math.Max(1, (int)Math.Abs(diff.TotalDays));
                        var notif = new ThongBao
                        {
                            NguoiNhanId = task.ThanhVienId.Value,
                            TieuDe = $"[KHẨN CẤP] Quá hạn {overdueDays} ngày: {comicName} Chap {chapNum}",
                            NoiDung = $"Công đoạn '{task.TenCongDoan}' đã quá hạn {overdueDays} ngày (Hạn chót: {task.HanChot:dd/MM/yyyy HH:mm}). Vui lòng cập nhật ngay!",
                            LoaiThongBao = "UrgentOverdue",
                            MucDo = "Danger",
                            CongDoanDeadlineId = task.Id,
                            NgayTao = now,
                            DaDoc = false
                        };
                        db.ThongBaos.Add(notif);
                    }
                }
                else if (diff.TotalHours <= 48)
                {
                    // Sắp hết hạn -> WarningExpiring
                    bool alreadySentWarning = db.ThongBaos.Any(t => 
                        t.CongDoanDeadlineId == task.Id && 
                        t.LoaiThongBao == "WarningExpiring" && 
                        t.NgayTao >= now.AddHours(-24));

                    if (!alreadySentWarning)
                    {
                        int hours = Math.Max(1, (int)diff.TotalHours);
                        var notif = new ThongBao
                        {
                            NguoiNhanId = task.ThanhVienId.Value,
                            TieuDe = $"[SẮP HẾT HẠN] {hours} giờ nữa: {comicName} Chap {chapNum}",
                            NoiDung = $"Hạn chót công đoạn '{task.TenCongDoan}' chỉ còn {hours} giờ ({task.HanChot:dd/MM/yyyy HH:mm}). Vui lòng hoàn thành đúng tiến độ!",
                            LoaiThongBao = "WarningExpiring",
                            MucDo = "Warning",
                            CongDoanDeadlineId = task.Id,
                            NgayTao = now,
                            DaDoc = false
                        };
                        db.ThongBaos.Add(notif);
                    }
                }
            }

            db.SaveChanges();
        }

        public List<ThongBao> GetNotificationsForUser(int? userId, int limit = 30)
        {
            using var db = new ComicDbContext();
            var query = db.ThongBaos
                .Include(t => t.CongDoanDeadline)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(t => t.NguoiNhanId == userId.Value || t.NguoiNhanId == null);
            }

            return query.OrderByDescending(t => t.NgayTao).Take(limit).ToList();
        }

        public int GetUnreadCount(int? userId)
        {
            using var db = new ComicDbContext();
            if (userId.HasValue)
            {
                return db.ThongBaos.Count(t => !t.DaDoc && (t.NguoiNhanId == userId.Value || t.NguoiNhanId == null));
            }
            return db.ThongBaos.Count(t => !t.DaDoc);
        }

        public void MarkAsRead(int notificationId)
        {
            using var db = new ComicDbContext();
            var notif = db.ThongBaos.Find(notificationId);
            if (notif != null)
            {
                notif.DaDoc = true;
                db.SaveChanges();
            }
        }

        public void MarkAllAsRead(int? userId)
        {
            using var db = new ComicDbContext();
            var list = db.ThongBaos.Where(t => !t.DaDoc).AsQueryable();
            if (userId.HasValue)
            {
                list = list.Where(t => t.NguoiNhanId == userId.Value || t.NguoiNhanId == null);
            }

            foreach (var item in list)
            {
                item.DaDoc = true;
            }
            db.SaveChanges();
        }

        public List<ThongBao> GetUrgentTasksForLoginPopup(int? userId)
        {
            using var db = new ComicDbContext();
            var query = db.ThongBaos
                .Include(t => t.CongDoanDeadline)
                .Where(t => !t.DaDoc && (t.MucDo == "Danger" || t.MucDo == "Warning"));

            if (userId.HasValue)
            {
                query = query.Where(t => t.NguoiNhanId == userId.Value || t.NguoiNhanId == null);
            }

            return query.OrderByDescending(t => t.NgayTao).Take(5).ToList();
        }
    }
}
