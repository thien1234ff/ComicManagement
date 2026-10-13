-- ===================================================================================
-- HỆ THỐNG QUẢN LÝ BIÊN TẬP TRUYỆN TRANH (Comic / Manga Editorial Studio Hub)
-- DATABASE INITIALIZATION SCRIPT (SQL Server)
-- ===================================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ComicManagementDb')
BEGIN
    CREATE DATABASE ComicManagementDb;
END
GO

USE ComicManagementDb;
GO

-- 1. BẢNG THÀNH VIÊN (ThanhVien)
IF OBJECT_ID(N'dbo.ThongBao', N'U') IS NOT NULL DROP TABLE dbo.ThongBao;
IF OBJECT_ID(N'dbo.CongDoanDeadline', N'U') IS NOT NULL DROP TABLE dbo.CongDoanDeadline;
IF OBJECT_ID(N'dbo.Chapter', N'U') IS NOT NULL DROP TABLE dbo.Chapter;
IF OBJECT_ID(N'dbo.Truyen', N'U') IS NOT NULL DROP TABLE dbo.Truyen;
IF OBJECT_ID(N'dbo.ThanhVien', N'U') IS NOT NULL DROP TABLE dbo.ThanhVien;
GO

CREATE TABLE dbo.ThanhVien (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhauHash NVARCHAR(256) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai NVARCHAR(20) NULL,
    DiscordTag NVARCHAR(50) NULL,
    VaiTroChuyenMon NVARCHAR(50) NOT NULL, -- 'Leader/Admin', 'Translator', 'Cleaner/Redraw', 'Typesetter', 'QC'
    TrangThaiHoatDong NVARCHAR(20) NOT NULL DEFAULT N'Active', -- 'Active', 'Busy', 'On Leave'
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2. BẢNG TRUYỆN (Truyen)
CREATE TABLE dbo.Truyen (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenTruyen NVARCHAR(200) NOT NULL,
    TacGia NVARCHAR(150) NOT NULL,
    TheLoai NVARCHAR(150) NOT NULL, -- Action, Shounen, Romance, Fantasy,...
    LoaiTruyen NVARCHAR(50) NOT NULL DEFAULT N'Manga', -- Manga, Manhwa, Comic
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đang tiến hành', -- 'Đang tiến hành', 'Tạm ngưng', 'Đã hoàn thành'
    FolderRawPsd NVARCHAR(500) NULL,
    HinhAnhBia NVARCHAR(500) NULL,
    MoTa NVARCHAR(MAX) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 3. BẢNG CHAPTER (Chapter)
CREATE TABLE dbo.Chapter (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TruyenId INT NOT NULL,
    SoChapter FLOAT NOT NULL,
    TieuDe NVARCHAR(200) NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đang biên tập', -- 'Chưa bắt đầu', 'Đang biên tập', 'Đã hoàn tất', 'Đã xuất bản'
    FolderPsd NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayXuatBan DATETIME2 NULL,
    CONSTRAINT FK_Chapter_Truyen FOREIGN KEY (TruyenId) REFERENCES dbo.Truyen(Id) ON DELETE CASCADE
);
GO

-- 4. BẢNG CÔNG ĐOẠN DEADLINE (CongDoanDeadline)
CREATE TABLE dbo.CongDoanDeadline (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ChapterId INT NOT NULL,
    TenCongDoan NVARCHAR(50) NOT NULL, -- 'Dịch (Trans)', 'Xóa thoại & Vẽ lại (Clean)', 'Điền thoại (Typeset)', 'Duyệt chất lượng (QC)'
    ThuTuPipeline INT NOT NULL DEFAULT 1,
    ThanhVienId INT NULL,
    NgayBatDau DATETIME2 NOT NULL DEFAULT GETDATE(),
    HanChot DATETIME2 NOT NULL,
    NgayHoanThanh DATETIME2 NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đang làm', -- 'Chưa nhận', 'Đang làm', 'Chờ duyệt', 'Hoàn thành'
    LinkNopSanPham NVARCHAR(500) NULL,
    GhiChu NVARCHAR(500) NULL,
    CONSTRAINT FK_CongDoanDeadline_Chapter FOREIGN KEY (ChapterId) REFERENCES dbo.Chapter(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CongDoanDeadline_ThanhVien FOREIGN KEY (ThanhVienId) REFERENCES dbo.ThanhVien(Id) ON DELETE SET NULL
);
GO

-- 5. BẢNG THÔNG BÁO (ThongBao)
CREATE TABLE dbo.ThongBao (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NguoiNhanId INT NULL, -- NULL là gửi đến toàn bộ hoặc admin
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    LoaiThongBao NVARCHAR(50) NOT NULL DEFAULT N'General', -- 'TaskAssigned', 'WarningExpiring', 'UrgentOverdue', 'General'
    MucDo NVARCHAR(20) NOT NULL DEFAULT N'Info', -- 'Info', 'Warning', 'Danger'
    DaDoc BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
    CongDoanDeadlineId INT NULL,
    CONSTRAINT FK_ThongBao_ThanhVien FOREIGN KEY (NguoiNhanId) REFERENCES dbo.ThanhVien(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ThongBao_CongDoan FOREIGN KEY (CongDoanDeadlineId) REFERENCES dbo.CongDoanDeadline(Id) ON DELETE SET NULL
);
GO

-- ===================================================================================
-- DỮ LIỆU SEED MẪU (Sample Seed Data)
-- ===================================================================================

-- 1. Insert Thành viên (Mật khẩu được lưu dạng SHA-256 hoặc plain text hash chuẩn hệ thống)
-- admin: admin123 -> SHA256: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
-- eren: eren123   -> SHA256: 010d86927bfdf8b8e053f0ee307d8ad4ff5bfb7c25e87a2d48c0818d2da53c9b
-- mikasa: mikasa123 -> SHA256: 80e75a963e69a08eec9b964d38ff13ab1e48398e09f7831f2214309e4d0d0186
-- levi: levi123   -> SHA256: e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855 (hoặc hash chuẩn)

INSERT INTO dbo.ThanhVien (HoTen, TenDangNhap, MatKhauHash, Email, SoDienThoai, DiscordTag, VaiTroChuyenMon, TrangThaiHoatDong, NgayTao)
VALUES 
(N'Kim Thiện (Editor-in-Chief)', N'admin', N'admin123', N'leader@comicstudio.vn', N'0909123456', N'LeaderKim#0001', N'Leader/Admin', N'Active', DATEADD(day, -60, GETDATE())),
(N'Eren Yeager', N'trans_eren', N'eren123', N'eren.trans@comicstudio.vn', N'0912345678', N'ErenTrans#1390', N'Translator', N'Active', DATEADD(day, -45, GETDATE())),
(N'Mikasa Ackerman', N'edit_mikasa', N'mikasa123', N'mikasa.clean@comicstudio.vn', N'0923456789', N'MikasaRedraw#2026', N'Cleaner/Redraw', N'Busy', DATEADD(day, -40, GETDATE())),
(N'Levi Ackerman', N'qc_levi', N'levi123', N'levi.qc@comicstudio.vn', N'0934567890', N'LeviCaptain#9999', N'QC', N'Active', DATEADD(day, -35, GETDATE()));

-- 2. Insert Truyện mẫu
INSERT INTO dbo.Truyen (TenTruyen, TacGia, TheLoai, LoaiTruyen, TrangThai, FolderRawPsd, HinhAnhBia, MoTa, NgayTao)
VALUES 
(N'One Piece: Đảo Hải Tặc', N'Eiichiro Oda', N'Shounen, Phiêu Lưu, Hài Hước, Giả Tưởng', N'Manga', N'Đang tiến hành', N'D:\StudioStorage\Manga\OnePiece\Raw', N'https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=600', N'Hành trình tìm kiếm kho báu huyền thoại One Piece và ước mơ trở thành Vua Hải Tặc của Monkey D. Luffy cùng băng Mũ Rơm.', DATEADD(day, -50, GETDATE())),
(N'Solo Leveling: Tôi Thăng Cấp Một Mình', N'Chugong, DUBU (REDICE)', N'Action, Hầm Ngục, Giả Tưởng, Hồi Sinh', N'Manhwa', N'Đang tiến hành', N'D:\StudioStorage\Manhwa\SoloLeveling\Raw', N'https://images.unsplash.com/photo-1578632767115-351597cf2477?w=600', N'Sung Jin-Woo, thợ săn cấp E yếu nhất thế giới, bất ngờ nhận được hệ thống tăng cấp bí ẩn sau trải nghiệm cận tử trong ngục kép.', DATEADD(day, -30, GETDATE())),
(N'Batman: The Dark Knight Returns', N'Frank Miller, Klaus Janson', N'Superhero, Trinh Thám, Noir, Hành Động', N'Comic', N'Đã hoàn thành', N'D:\StudioStorage\Comics\Batman\PSD', N'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=600', N'Kiệt tác truyện tranh kinh điển kể về sự trở lại của Hiệp sĩ Bóng đêm Bruce Wayne sau 10 năm giải nghệ để cứu lấy Gotham.', DATEADD(day, -90, GETDATE()));

-- 3. Insert Chapter mẫu
-- One Piece: Chap 1115, 1116, 1117
INSERT INTO dbo.Chapter (TruyenId, SoChapter, TieuDe, TrangThai, FolderPsd, NgayTao, NgayXuatBan)
VALUES 
(1, 1115, N'Mảnh Ghép Lịch Sử Thế Giới', N'Đã xuất bản', N'D:\Storage\OP\1115', DATEADD(day, -15, GETDATE()), DATEADD(day, -10, GETDATE())),
(1, 1116, N'Thông Điệp Từ Tiến Sĩ Vegapunk', N'Đang biên tập', N'D:\Storage\OP\1116', DATEADD(day, -5, GETDATE()), NULL),
(1, 1117, N'Hội Tụ Tại Đảo Trứng Egghead', N'Đang biên tập', N'D:\Storage\OP\1117', DATEADD(day, -1, GETDATE()), NULL);

-- Solo Leveling: Chap 177, 178, 179, 180
INSERT INTO dbo.Chapter (TruyenId, SoChapter, TieuDe, TrangThai, FolderPsd, NgayTao, NgayXuatBan)
VALUES 
(2, 177, N'Đại Chiến Các Hoàng Đế', N'Đã xuất bản', N'D:\Storage\SL\177', DATEADD(day, -20, GETDATE()), DATEADD(day, -14, GETDATE())),
(2, 178, N'Thời Khắc Của Sự Thức Tỉnh', N'Đang biên tập', N'D:\Storage\SL\178', DATEADD(day, -8, GETDATE()), NULL),
(2, 179, N'Sức Mạnh Quân Đoàn Bóng Tối', N'Đang biên tập', N'D:\Storage\SL\179', DATEADD(day, -3, GETDATE()), NULL),
(2, 180, N'Hồi Kết Vinh Quang', N'Chưa bắt đầu', N'D:\Storage\SL\180', GETDATE(), NULL);

-- Batman: Chap 1, 2, 3
INSERT INTO dbo.Chapter (TruyenId, SoChapter, TieuDe, TrangThai, FolderPsd, NgayTao, NgayXuatBan)
VALUES 
(3, 1, N'The Dark Knight Falls', N'Đã xuất bản', N'D:\Storage\BM\Ch1', DATEADD(day, -80, GETDATE()), DATEADD(day, -75, GETDATE())),
(3, 2, N'The Dark Knight Triumphant', N'Đã xuất bản', N'D:\Storage\BM\Ch2', DATEADD(day, -70, GETDATE()), DATEADD(day, -65, GETDATE())),
(3, 3, N'Hunt the Dark Knight', N'Đã hoàn tất', N'D:\Storage\BM\Ch3', DATEADD(day, -4, GETDATE()), NULL);

-- 4. Insert Công Đoạn Deadline (Mô phỏng đầy đủ: Đã hoàn thành, Đang làm an toàn, Sắp trễ, Quá hạn)
-- Cho One Piece Chap 1116:
-- + Dịch: Đã hoàn thành
-- + Clean/Redraw: Quá hạn 2 ngày! (Overdue - Cảnh báo ĐỎ)
-- + Typeset: Đang làm, sắp hết hạn trong 18 giờ! (Warning - VÀNG/CAM)
-- + QC: Chưa nhận, hạn 3 ngày tới (XANH AN TOÀN)
INSERT INTO dbo.CongDoanDeadline (ChapterId, TenCongDoan, ThuTuPipeline, ThanhVienId, NgayBatDau, HanChot, NgayHoanThanh, TrangThai, LinkNopSanPham, GhiChu)
VALUES 
(2, N'Dịch (Trans)', 1, 2, DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/op1116-trans.docx', N'Dịch sát thuật ngữ Wano và Egghead'),
(2, N'Xóa thoại & Vẽ lại (Clean)', 2, 3, DATEADD(day, -4, GETDATE()), DATEADD(day, -2, GETDATE()), NULL, N'Đang làm', NULL, N'Trang đôi 12-13 bị nhiều SFX rắc rối, đang trễ tiến độ!'),
(2, N'Điền thoại (Typeset)', 3, 3, DATEADD(day, -2, GETDATE()), DATEADD(hour, 18, GETDATE()), NULL, N'Đang làm', NULL, N'Chờ file Clean hoàn thiện để merge bóng thoại'),
(2, N'Duyệt chất lượng (QC)', 4, 4, DATEADD(day, -1, GETDATE()), DATEADD(day, 3, GETDATE()), NULL, N'Chưa nhận', NULL, N'Kiểm tra lỗi type và khớp khung hình');

-- Cho One Piece Chap 1117:
INSERT INTO dbo.CongDoanDeadline (ChapterId, TenCongDoan, ThuTuPipeline, ThanhVienId, NgayBatDau, HanChot, NgayHoanThanh, TrangThai, LinkNopSanPham, GhiChu)
VALUES 
(3, N'Dịch (Trans)', 1, 2, DATEADD(day, -1, GETDATE()), DATEADD(day, 2, GETDATE()), NULL, N'Đang làm', NULL, N'Bản Scan Raw chất lượng cao của TCB Scans'),
(3, N'Xóa thoại & Vẽ lại (Clean)', 2, 3, GETDATE(), DATEADD(day, 4, GETDATE()), NULL, N'Chưa nhận', NULL, N''),
(3, N'Điền thoại (Typeset)', 3, NULL, GETDATE(), DATEADD(day, 5, GETDATE()), NULL, N'Chưa nhận', NULL, N''),
(3, N'Duyệt chất lượng (QC)', 4, 4, GETDATE(), DATEADD(day, 6, GETDATE()), NULL, N'Chưa nhận', NULL, N'');

-- Cho Solo Leveling Chap 178:
-- + Dịch: Hoàn thành
-- + Clean: Hoàn thành
-- + Typeset: Quá hạn 1 ngày (ĐỎ)
-- + QC: Đang chờ duyệt
INSERT INTO dbo.CongDoanDeadline (ChapterId, TenCongDoan, ThuTuPipeline, ThanhVienId, NgayBatDau, HanChot, NgayHoanThanh, TrangThai, LinkNopSanPham, GhiChu)
VALUES 
(5, N'Dịch (Trans)', 1, 2, DATEADD(day, -6, GETDATE()), DATEADD(day, -4, GETDATE()), DATEADD(day, -4, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/sl178-trans', N''),
(5, N'Xóa thoại & Vẽ lại (Clean)', 2, 3, DATEADD(day, -4, GETDATE()), DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/sl178-clean.psd', N'Redraw hiệu ứng tia sét và bóng tối mượt mà'),
(5, N'Điền thoại (Typeset)', 3, 3, DATEADD(day, -3, GETDATE()), DATEADD(day, -1, GETDATE()), NULL, N'Đang làm', NULL, N'Cần font SFX hành động Hàn Quốc'),
(5, N'Duyệt chất lượng (QC)', 4, 4, DATEADD(day, -1, GETDATE()), DATEADD(hour, 36, GETDATE()), NULL, N'Chờ duyệt', N'https://drive.google.com/drive/u/0/sl178-draft-pdf', N'Đang rà soát chính tả tên chiêu thức');

-- Cho Batman Chap 3:
INSERT INTO dbo.CongDoanDeadline (ChapterId, TenCongDoan, ThuTuPipeline, ThanhVienId, NgayBatDau, HanChot, NgayHoanThanh, TrangThai, LinkNopSanPham, GhiChu)
VALUES 
(9, N'Dịch (Trans)', 1, 2, DATEADD(day, -4, GETDATE()), DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/bm3-trans', N''),
(9, N'Xóa thoại & Vẽ lại (Clean)', 2, 3, DATEADD(day, -3, GETDATE()), DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/bm3-clean', N''),
(9, N'Điền thoại (Typeset)', 3, 3, DATEADD(day, -2, GETDATE()), DATEADD(day, -1, GETDATE()), DATEADD(day, -1, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/bm3-typeset', N''),
(9, N'Duyệt chất lượng (QC)', 4, 1, DATEADD(day, -1, GETDATE()), DATEADD(day, 1, GETDATE()), DATEADD(day, -1, GETDATE()), N'Hoàn thành', N'https://drive.google.com/drive/u/0/bm3-final-cbr', N'Leader đã ký duyệt xuất bản');

-- 5. Insert Thông Báo mẫu
INSERT INTO dbo.ThongBao (NguoiNhanId, TieuDe, NoiDung, LoaiThongBao, MucDo, DaDoc, NgayTao, CongDoanDeadlineId)
VALUES 
(3, N'[KHẨN CẤP] Quá hạn Clean: One Piece Chap 1116', N'Công đoạn Clean/Redraw cho One Piece Chap 1116 đã quá hạn 2 ngày. Vui lòng cập nhật tiến độ hoặc liên hệ Leader!', N'UrgentOverdue', N'Danger', 0, DATEADD(hour, -12, GETDATE()), 2),
(3, N'[SẮP HẾT HẠN] Typeset: One Piece Chap 1116', N'Hạn chót công đoạn Typeset chỉ còn dưới 18 giờ. Hãy hoàn thành sớm để chuyển sang bước QC.', N'WarningExpiring', N'Warning', 0, DATEADD(hour, -4, GETDATE()), 3),
(3, N'[KHẨN CẤP] Quá hạn Typeset: Solo Leveling Chap 178', N'Công đoạn Typeset cho Solo Leveling Chap 178 đã quá hạn 1 ngày. Vui lòng nộp file draft!', N'UrgentOverdue', N'Danger', 0, DATEADD(hour, -8, GETDATE()), 7),
(2, N'[DEADLINE MỚI] Dịch: One Piece Chap 1117', N'Bạn đã được phân công dịch One Piece Chap 1117. Hạn chót còn 2 ngày.', N'TaskAssigned', N'Info', 0, DATEADD(day, -1, GETDATE()), 5),
(NULL, N'Chào mừng thành viên đến với Manga Editorial Studio Hub!', N'Hệ thống quản lý biên tập truyện tranh phiên bản 2026 đã sẵn sàng phục vụ đội ngũ sáng tạo.', N'General', N'Info', 1, DATEADD(day, -30, GETDATE()), NULL);
GO

PRINT N'Database ComicManagementDb initialized with seed data successfully!';
