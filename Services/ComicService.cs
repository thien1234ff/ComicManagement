using System;
using System.Collections.Generic;
using System.Linq;
using ComicManagement.Data;
using ComicManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicManagement.Services
{
    public class ComicService
    {
        public List<Truyen> GetAllComics(string? keyword = null, string? theLoai = null, string? trangThai = null)
        {
            using var db = new ComicDbContext();
            var query = db.Truyens
                .Include(t => t.Chapters)
                    .ThenInclude(c => c.CacCongDoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(t => t.TenTruyen.ToLower().Contains(kw) ||
                                         t.TacGia.ToLower().Contains(kw) ||
                                         t.TheLoai.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(theLoai) && theLoai != "Tất cả")
            {
                query = query.Where(t => t.TheLoai.Contains(theLoai) || t.LoaiTruyen == theLoai);
            }

            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            return query.OrderByDescending(t => t.NgayTao).ToList();
        }

        public Truyen? GetComicById(int id)
        {
            using var db = new ComicDbContext();
            return db.Truyens
                .Include(t => t.Chapters)
                    .ThenInclude(c => c.CacCongDoan)
                        .ThenInclude(cd => cd.ThanhVien)
                .FirstOrDefault(t => t.Id == id);
        }

        public (bool Success, string Message, Truyen? Comic) AddComic(Truyen comic)
        {
            if (string.IsNullOrWhiteSpace(comic.TenTruyen))
            {
                return (false, "Tên truyện không được để trống!", null);
            }

            using var db = new ComicDbContext();
            comic.NgayTao = DateTime.Now;
            db.Truyens.Add(comic);
            db.SaveChanges();
            return (true, "Thêm truyện thành công!", comic);
        }

        public (bool Success, string Message) UpdateComic(Truyen comic)
        {
            using var db = new ComicDbContext();
            var existing = db.Truyens.Find(comic.Id);
            if (existing == null)
            {
                return (false, "Không tìm thấy bộ truyện!");
            }

            existing.TenTruyen = comic.TenTruyen;
            existing.TacGia = comic.TacGia;
            existing.TheLoai = comic.TheLoai;
            existing.LoaiTruyen = comic.LoaiTruyen;
            existing.TrangThai = comic.TrangThai;
            existing.FolderRawPsd = comic.FolderRawPsd;
            existing.HinhAnhBia = comic.HinhAnhBia;
            existing.MoTa = comic.MoTa;

            db.SaveChanges();
            return (true, "Cập nhật thông tin truyện thành công!");
        }

        public (bool Success, string Message) UpdateComicStatus(int comicId, string status)
        {
            using var db = new ComicDbContext();
            var existing = db.Truyens.Find(comicId);
            if (existing == null) return (false, "Không tìm thấy bộ truyện!");
            existing.TrangThai = status;
            db.SaveChanges();
            return (true, $"Đã chuyển trạng thái truyện sang '{status}'!");
        }

        public (bool Success, string Message) DeleteComic(int id)
        {
            using var db = new ComicDbContext();
            var comic = db.Truyens.Find(id);
            if (comic == null)
            {
                return (false, "Không tìm thấy bộ truyện!");
            }

            db.Truyens.Remove(comic);
            db.SaveChanges();
            return (true, "Đã xóa truyện thành công!");
        }

        // --- CHAPTER OPERATIONS ---

        public List<Chapter> GetChaptersByComicId(int truyenId)
        {
            using var db = new ComicDbContext();
            return db.Chapters
                .Include(c => c.CacCongDoan)
                    .ThenInclude(cd => cd.ThanhVien)
                .Where(c => c.TruyenId == truyenId)
                .OrderBy(c => c.SoChapter)
                .ToList();
        }

        public (bool Success, string Message, Chapter? Chapter) AddChapter(Chapter chapter, bool autoCreateDefaultPipelines = true)
        {
            using var db = new ComicDbContext();
            chapter.NgayTao = DateTime.Now;
            db.Chapters.Add(chapter);
            db.SaveChanges();

            if (autoCreateDefaultPipelines)
            {
                // Tự động tạo 4 công đoạn chuẩn: Trans, Clean, Typeset, QC
                var stages = new[]
                {
                    (PipelineStages.Translate, 1, 2),
                    (PipelineStages.CleanRedraw, 2, 3),
                    (PipelineStages.Typeset, 3, 4),
                    (PipelineStages.QualityCheck, 4, 5)
                };

                foreach (var (stageName, order, days) in stages)
                {
                    var cd = new CongDoanDeadline
                    {
                        ChapterId = chapter.Id,
                        TenCongDoan = stageName,
                        ThuTuPipeline = order,
                        NgayBatDau = DateTime.Now,
                        HanChot = DateTime.Now.AddDays(days),
                        TrangThai = TaskStatuses.NotStarted
                    };
                    db.CongDoanDeadlines.Add(cd);
                }
                db.SaveChanges();
            }

            return (true, "Thêm Chapter thành công!", chapter);
        }

        public (bool Success, string Message) UpdateChapter(Chapter chapter)
        {
            using var db = new ComicDbContext();
            var existing = db.Chapters.Find(chapter.Id);
            if (existing == null) return (false, "Không tìm thấy Chapter!");

            existing.SoChapter = chapter.SoChapter;
            existing.TieuDe = chapter.TieuDe;
            existing.TrangThai = chapter.TrangThai;
            existing.FolderPsd = chapter.FolderPsd;
            if (chapter.TrangThai == "Đã xuất bản" && existing.NgayXuatBan == null)
            {
                existing.NgayXuatBan = DateTime.Now;
            }

            db.SaveChanges();
            return (true, "Cập nhật Chapter thành công!");
        }

        public (bool Success, string Message) UpdateChapterStatus(int chapterId, string status)
        {
            using var db = new ComicDbContext();
            var existing = db.Chapters.Find(chapterId);
            if (existing == null) return (false, "Không tìm thấy Chapter!");

            existing.TrangThai = status;
            if (status == "Đã xuất bản" && existing.NgayXuatBan == null)
            {
                existing.NgayXuatBan = DateTime.Now;
            }

            db.SaveChanges();
            return (true, $"Đã cập nhật trạng thái Chapter thành '{status}'!");
        }

        public (bool Success, string Message) DeleteChapter(int id)
        {
            using var db = new ComicDbContext();
            var chapter = db.Chapters.Find(id);
            if (chapter == null) return (false, "Không tìm thấy Chapter!");

            db.Chapters.Remove(chapter);
            db.SaveChanges();
            return (true, "Đã xóa Chapter thành công!");
        }
    }
}
