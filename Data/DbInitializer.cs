using System;
using System.Collections.Generic;
using System.Linq;
using ComicManagement.Models;

namespace ComicManagement.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ComicDbContext context)
        {
            // Đảm bảo database được tạo
            context.Database.EnsureCreated();

            // Kiểm tra xem đã có dữ liệu hay chưa
            if (context.ThanhViens.Any())
            {
                return; // Đã có dữ liệu
            }

            // 1. Seed Thành viên
            var admin = new ThanhVien
            {
                HoTen = "Kim Thiện (Editor-in-Chief)",
                TenDangNhap = "admin",
                MatKhauHash = "admin123",
                Email = "leader@comicstudio.vn",
                SoDienThoai = "0909123456",
                DiscordTag = "LeaderKim#0001",
                VaiTroChuyenMon = StudioRoles.LeaderAdmin,
                TrangThaiHoatDong = MemberStatuses.Active,
                NgayTao = DateTime.Now.AddDays(-60)
            };

            var eren = new ThanhVien
            {
                HoTen = "Eren Yeager",
                TenDangNhap = "trans_eren",
                MatKhauHash = "eren123",
                Email = "eren.trans@comicstudio.vn",
                SoDienThoai = "0912345678",
                DiscordTag = "ErenTrans#1390",
                VaiTroChuyenMon = StudioRoles.Translator,
                TrangThaiHoatDong = MemberStatuses.Active,
                NgayTao = DateTime.Now.AddDays(-45)
            };

            var mikasa = new ThanhVien
            {
                HoTen = "Mikasa Ackerman",
                TenDangNhap = "edit_mikasa",
                MatKhauHash = "mikasa123",
                Email = "mikasa.clean@comicstudio.vn",
                SoDienThoai = "0923456789",
                DiscordTag = "MikasaRedraw#2026",
                VaiTroChuyenMon = StudioRoles.CleanerRedraw,
                TrangThaiHoatDong = MemberStatuses.Busy,
                NgayTao = DateTime.Now.AddDays(-40)
            };

            var levi = new ThanhVien
            {
                HoTen = "Levi Ackerman",
                TenDangNhap = "qc_levi",
                MatKhauHash = "levi123",
                Email = "levi.qc@comicstudio.vn",
                SoDienThoai = "0934567890",
                DiscordTag = "LeviCaptain#9999",
                VaiTroChuyenMon = StudioRoles.QC,
                TrangThaiHoatDong = MemberStatuses.Active,
                NgayTao = DateTime.Now.AddDays(-35)
            };

            context.ThanhViens.AddRange(admin, eren, mikasa, levi);
            context.SaveChanges();

            // 2. Seed Truyện
            var op = new Truyen
            {
                TenTruyen = "One Piece: Đảo Hải Tặc",
                TacGia = "Eiichiro Oda",
                TheLoai = "Shounen, Phiêu Lưu, Hài Hước, Giả Tưởng",
                LoaiTruyen = "Manga",
                TrangThai = ComicStatuses.InProgress,
                FolderRawPsd = @"D:\StudioStorage\Manga\OnePiece\Raw",
                HinhAnhBia = "https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=600",
                MoTa = "Hành trình tìm kiếm kho báu huyền thoại One Piece và ước mơ trở thành Vua Hải Tặc của Monkey D. Luffy cùng băng Mũ Rơm.",
                NgayTao = DateTime.Now.AddDays(-50)
            };

            var sl = new Truyen
            {
                TenTruyen = "Solo Leveling: Tôi Thăng Cấp Một Mình",
                TacGia = "Chugong, DUBU (REDICE)",
                TheLoai = "Action, Hầm Ngục, Giả Tưởng, Hồi Sinh",
                LoaiTruyen = "Manhwa",
                TrangThai = ComicStatuses.InProgress,
                FolderRawPsd = @"D:\StudioStorage\Manhwa\SoloLeveling\Raw",
                HinhAnhBia = "https://images.unsplash.com/photo-1578632767115-351597cf2477?w=600",
                MoTa = "Sung Jin-Woo, thợ săn cấp E yếu nhất thế giới, bất ngờ nhận được hệ thống tăng cấp bí ẩn sau trải nghiệm cận tử trong ngục kép.",
                NgayTao = DateTime.Now.AddDays(-30)
            };

            var bm = new Truyen
            {
                TenTruyen = "Batman: The Dark Knight Returns",
                TacGia = "Frank Miller, Klaus Janson",
                TheLoai = "Superhero, Trinh Thám, Noir, Hành Động",
                LoaiTruyen = "Comic",
                TrangThai = ComicStatuses.Completed,
                FolderRawPsd = @"D:\StudioStorage\Comics\Batman\PSD",
                HinhAnhBia = "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=600",
                MoTa = "Kiệt tác truyện tranh kinh điển kể về sự trở lại của Hiệp sĩ Bóng đêm Bruce Wayne sau 10 năm giải nghệ để cứu lấy Gotham.",
                NgayTao = DateTime.Now.AddDays(-90)
            };

            context.Truyens.AddRange(op, sl, bm);
            context.SaveChanges();

            // 3. Seed Chapters
            // One Piece
            var op1 = new Chapter { TruyenId = op.Id, SoChapter = 1115, TieuDe = "Mảnh Ghép Lịch Sử Thế Giới", TrangThai = "Đã xuất bản", NgayTao = DateTime.Now.AddDays(-15), NgayXuatBan = DateTime.Now.AddDays(-10) };
            var op2 = new Chapter { TruyenId = op.Id, SoChapter = 1116, TieuDe = "Thông Điệp Từ Tiến Sĩ Vegapunk", TrangThai = "Đang biên tập", NgayTao = DateTime.Now.AddDays(-5) };
            var op3 = new Chapter { TruyenId = op.Id, SoChapter = 1117, TieuDe = "Hội Tụ Tại Đảo Trứng Egghead", TrangThai = "Đang biên tập", NgayTao = DateTime.Now.AddDays(-1) };

            // Solo Leveling
            var sl1 = new Chapter { TruyenId = sl.Id, SoChapter = 177, TieuDe = "Đại Chiến Các Hoàng Đế", TrangThai = "Đã xuất bản", NgayTao = DateTime.Now.AddDays(-20), NgayXuatBan = DateTime.Now.AddDays(-14) };
            var sl2 = new Chapter { TruyenId = sl.Id, SoChapter = 178, TieuDe = "Thời Khắc Của Sự Thức Tỉnh", TrangThai = "Đang biên tập", NgayTao = DateTime.Now.AddDays(-8) };
            var sl3 = new Chapter { TruyenId = sl.Id, SoChapter = 179, TieuDe = "Sức Mạnh Quân Đoàn Bóng Tối", TrangThai = "Đang biên tập", NgayTao = DateTime.Now.AddDays(-3) };
            var sl4 = new Chapter { TruyenId = sl.Id, SoChapter = 180, TieuDe = "Hồi Kết Vinh Quang", TrangThai = "Chưa bắt đầu", NgayTao = DateTime.Now };

            // Batman
            var bm1 = new Chapter { TruyenId = bm.Id, SoChapter = 1, TieuDe = "The Dark Knight Falls", TrangThai = "Đã xuất bản", NgayTao = DateTime.Now.AddDays(-80), NgayXuatBan = DateTime.Now.AddDays(-75) };
            var bm2 = new Chapter { TruyenId = bm.Id, SoChapter = 2, TieuDe = "The Dark Knight Triumphant", TrangThai = "Đã xuất bản", NgayTao = DateTime.Now.AddDays(-70), NgayXuatBan = DateTime.Now.AddDays(-65) };
            var bm3 = new Chapter { TruyenId = bm.Id, SoChapter = 3, TieuDe = "Hunt the Dark Knight", TrangThai = "Đã hoàn tất", NgayTao = DateTime.Now.AddDays(-4) };

            context.Chapters.AddRange(op1, op2, op3, sl1, sl2, sl3, sl4, bm1, bm2, bm3);
            context.SaveChanges();

            // 4. Seed Công đoạn Deadline
            var deadlines = new List<CongDoanDeadline>
            {
                // One Piece 1116
                new CongDoanDeadline
                {
                    ChapterId = op2.Id,
                    TenCongDoan = PipelineStages.Translate,
                    ThuTuPipeline = 1,
                    ThanhVienId = eren.Id,
                    NgayBatDau = DateTime.Now.AddDays(-5),
                    HanChot = DateTime.Now.AddDays(-3),
                    NgayHoanThanh = DateTime.Now.AddDays(-3),
                    TrangThai = TaskStatuses.Completed,
                    LinkNopSanPham = "https://drive.google.com/drive/u/0/op1116-trans.docx",
                    GhiChu = "Dịch sát thuật ngữ Wano và Egghead"
                },
                new CongDoanDeadline
                {
                    ChapterId = op2.Id,
                    TenCongDoan = PipelineStages.CleanRedraw,
                    ThuTuPipeline = 2,
                    ThanhVienId = mikasa.Id,
                    NgayBatDau = DateTime.Now.AddDays(-4),
                    HanChot = DateTime.Now.AddDays(-2), // QUÁ HẠN 2 NGÀY!
                    TrangThai = TaskStatuses.InProgress,
                    GhiChu = "Trang đôi 12-13 bị nhiều SFX rắc rối, đang trễ tiến độ!"
                },
                new CongDoanDeadline
                {
                    ChapterId = op2.Id,
                    TenCongDoan = PipelineStages.Typeset,
                    ThuTuPipeline = 3,
                    ThanhVienId = mikasa.Id,
                    NgayBatDau = DateTime.Now.AddDays(-2),
                    HanChot = DateTime.Now.AddHours(18), // SẮP HẾT HẠN (18H)
                    TrangThai = TaskStatuses.InProgress,
                    GhiChu = "Chờ file Clean hoàn thiện để merge bóng thoại"
                },
                new CongDoanDeadline
                {
                    ChapterId = op2.Id,
                    TenCongDoan = PipelineStages.QualityCheck,
                    ThuTuPipeline = 4,
                    ThanhVienId = levi.Id,
                    NgayBatDau = DateTime.Now.AddDays(-1),
                    HanChot = DateTime.Now.AddDays(3), // AN TOÀN (> 2 NGÀY)
                    TrangThai = TaskStatuses.NotStarted,
                    GhiChu = "Kiểm tra lỗi type và khớp khung hình"
                },

                // One Piece 1117
                new CongDoanDeadline
                {
                    ChapterId = op3.Id,
                    TenCongDoan = PipelineStages.Translate,
                    ThuTuPipeline = 1,
                    ThanhVienId = eren.Id,
                    NgayBatDau = DateTime.Now.AddDays(-1),
                    HanChot = DateTime.Now.AddDays(2),
                    TrangThai = TaskStatuses.InProgress,
                    GhiChu = "Bản Scan Raw chất lượng cao của TCB Scans"
                },
                new CongDoanDeadline
                {
                    ChapterId = op3.Id,
                    TenCongDoan = PipelineStages.CleanRedraw,
                    ThuTuPipeline = 2,
                    ThanhVienId = mikasa.Id,
                    NgayBatDau = DateTime.Now,
                    HanChot = DateTime.Now.AddDays(4),
                    TrangThai = TaskStatuses.NotStarted
                },

                // Solo Leveling 178
                new CongDoanDeadline
                {
                    ChapterId = sl2.Id,
                    TenCongDoan = PipelineStages.Translate,
                    ThuTuPipeline = 1,
                    ThanhVienId = eren.Id,
                    NgayBatDau = DateTime.Now.AddDays(-6),
                    HanChot = DateTime.Now.AddDays(-4),
                    NgayHoanThanh = DateTime.Now.AddDays(-4),
                    TrangThai = TaskStatuses.Completed,
                    LinkNopSanPham = "https://drive.google.com/drive/u/0/sl178-trans"
                },
                new CongDoanDeadline
                {
                    ChapterId = sl2.Id,
                    TenCongDoan = PipelineStages.CleanRedraw,
                    ThuTuPipeline = 2,
                    ThanhVienId = mikasa.Id,
                    NgayBatDau = DateTime.Now.AddDays(-4),
                    HanChot = DateTime.Now.AddDays(-2),
                    NgayHoanThanh = DateTime.Now.AddDays(-2),
                    TrangThai = TaskStatuses.Completed,
                    LinkNopSanPham = "https://drive.google.com/drive/u/0/sl178-clean.psd",
                    GhiChu = "Redraw hiệu ứng tia sét và bóng tối mượt mà"
                },
                new CongDoanDeadline
                {
                    ChapterId = sl2.Id,
                    TenCongDoan = PipelineStages.Typeset,
                    ThuTuPipeline = 3,
                    ThanhVienId = mikasa.Id,
                    NgayBatDau = DateTime.Now.AddDays(-3),
                    HanChot = DateTime.Now.AddDays(-1), // QUÁ HẠN 1 NGÀY
                    TrangThai = TaskStatuses.InProgress,
                    GhiChu = "Cần font SFX hành động Hàn Quốc"
                },
                new CongDoanDeadline
                {
                    ChapterId = sl2.Id,
                    TenCongDoan = PipelineStages.QualityCheck,
                    ThuTuPipeline = 4,
                    ThanhVienId = levi.Id,
                    NgayBatDau = DateTime.Now.AddDays(-1),
                    HanChot = DateTime.Now.AddHours(36), // SẮP HẾT HẠN (36H)
                    TrangThai = TaskStatuses.PendingApproval,
                    LinkNopSanPham = "https://drive.google.com/drive/u/0/sl178-draft-pdf",
                    GhiChu = "Đang rà soát chính tả tên chiêu thức"
                },

                // Batman Chap 3
                new CongDoanDeadline
                {
                    ChapterId = bm3.Id,
                    TenCongDoan = PipelineStages.QualityCheck,
                    ThuTuPipeline = 4,
                    ThanhVienId = admin.Id,
                    NgayBatDau = DateTime.Now.AddDays(-1),
                    HanChot = DateTime.Now.AddDays(1),
                    NgayHoanThanh = DateTime.Now.AddDays(-1),
                    TrangThai = TaskStatuses.Completed,
                    LinkNopSanPham = "https://drive.google.com/drive/u/0/bm3-final-cbr",
                    GhiChu = "Leader đã ký duyệt xuất bản"
                }
            };

            context.CongDoanDeadlines.AddRange(deadlines);
            context.SaveChanges();

            // 5. Seed Thông Báo
            var notifs = new List<ThongBao>
            {
                new ThongBao
                {
                    NguoiNhanId = mikasa.Id,
                    TieuDe = "[KHẨN CẤP] Quá hạn Clean: One Piece Chap 1116",
                    NoiDung = "Công đoạn Clean/Redraw cho One Piece Chap 1116 đã quá hạn 2 ngày. Vui lòng cập nhật tiến độ hoặc liên hệ Leader!",
                    LoaiThongBao = "UrgentOverdue",
                    MucDo = "Danger",
                    DaDoc = false,
                    NgayTao = DateTime.Now.AddHours(-12),
                    CongDoanDeadlineId = deadlines[1].Id
                },
                new ThongBao
                {
                    NguoiNhanId = mikasa.Id,
                    TieuDe = "[SẮP HẾT HẠN] Typeset: One Piece Chap 1116",
                    NoiDung = "Hạn chót công đoạn Typeset chỉ còn dưới 18 giờ. Hãy hoàn thành sớm để chuyển sang bước QC.",
                    LoaiThongBao = "WarningExpiring",
                    MucDo = "Warning",
                    DaDoc = false,
                    NgayTao = DateTime.Now.AddHours(-4),
                    CongDoanDeadlineId = deadlines[2].Id
                },
                new ThongBao
                {
                    NguoiNhanId = mikasa.Id,
                    TieuDe = "[KHẨN CẤP] Quá hạn Typeset: Solo Leveling Chap 178",
                    NoiDung = "Công đoạn Typeset cho Solo Leveling Chap 178 đã quá hạn 1 ngày. Vui lòng nộp file draft!",
                    LoaiThongBao = "UrgentOverdue",
                    MucDo = "Danger",
                    DaDoc = false,
                    NgayTao = DateTime.Now.AddHours(-8),
                    CongDoanDeadlineId = deadlines[8].Id
                },
                new ThongBao
                {
                    NguoiNhanId = eren.Id,
                    TieuDe = "[DEADLINE MỚI] Dịch: One Piece Chap 1117",
                    NoiDung = "Bạn đã được phân công dịch One Piece Chap 1117. Hạn chót còn 2 ngày.",
                    LoaiThongBao = "TaskAssigned",
                    MucDo = "Info",
                    DaDoc = false,
                    NgayTao = DateTime.Now.AddDays(-1),
                    CongDoanDeadlineId = deadlines[4].Id
                },
                new ThongBao
                {
                    NguoiNhanId = null,
                    TieuDe = "Chào mừng thành viên đến với Manga Editorial Studio Hub!",
                    NoiDung = "Hệ thống quản lý biên tập truyện tranh phiên bản 2026 đã sẵn sàng phục vụ đội ngũ sáng tạo.",
                    LoaiThongBao = "General",
                    MucDo = "Info",
                    DaDoc = true,
                    NgayTao = DateTime.Now.AddDays(-30)
                }
            };

            context.ThongBaos.AddRange(notifs);
            context.SaveChanges();
        }
    }
}
