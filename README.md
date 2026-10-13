# 🎨 MANGA / WEBTOON CREATIVE EDITORIAL STUDIO HUB (2026)
> **Hệ thống Quản lý Biên tập Truyện tranh Chuyên nghiệp**  
> Xây dựng trên nền tảng **C# .NET 8 Windows Forms** & **Entity Framework Core 8.0** kết hợp **Microsoft SQL Server (LocalDB / SQLEXPRESS)**.

---

## 🌟 TỔNG QUAN KIẾN TRÚC & PHONG CÁCH THIẾT KẾ (STUDIO DARK-CREATIVE)

Ứng dụng được thiết kế hoàn toàn khác biệt với các phần mềm doanh nghiệp thông thường, mang phong cách sáng tạo của các Studio Manga/Webtoon hàng đầu (Nhật Bản & Hàn Quốc):
- **Bảng màu chủ đạo**: Dark Slate (`#181825`, `#1E1E2E`, `#252538`).
- **Điểm nhấn Cyber & Neon**: Tím Neon (`#7C3AED`, `#8B5CF6`), Hồng Neon (`#EC4899`), Cyan (`#06B6D4`), Xanh Emerald (`#10B981`), Vàng Amber (`#F59E0B`), Đỏ Cảnh Báo (`#EF4444`).
- **Cover Card UI**: Hiển thị truyện dạng bìa truyện bo góc, tag thể loại, loại truyện (Manga/Manhwa/Comic) và thanh tiến độ hoàn thành (% Progress Bar).
- **Kanban Timeline Pipeline 6 Cột**: Mô phỏng thực tế quy trình biên tập:
  `[1. Raw/Scan]` ➔ `[2. Dịch/Trans]` ➔ `[3. Xóa thoại/Clean]` ➔ `[4. Điền chữ/Typeset]` ➔ `[5. Duyệt/QC]` ➔ `[6. Hoàn tất]`.
- **Cảnh báo Deadline trực quan bằng màu sắc**:
  - 🟢 **Xanh lá an toàn**: Còn hạn `> 2 ngày`
  - 🟠 **Vàng/Cam cảnh báo**: Sắp hết hạn `<= 48 giờ`
  - 🔴 **Đỏ rực khẩn cấp**: Quá hạn (Overdue) kèm số ngày trễ

---

## 🗂️ KIẾN TRÚC MÃ NGUỒN (CLEAN ARCHITECTURE)

```
ComicManagement/
│
├── Models/                     # Entity Dữ liệu & Quy tắc miền (Domain Entities)
│   ├── ThanhVien.cs            # Thành viên & Phân quyền Studio
│   ├── Truyen.cs               # Bộ truyện (Manga, Manhwa, Comic)
│   ├── Chapter.cs              # Tập/Chương truyện
│   ├── CongDoanDeadline.cs     # Công đoạn Deadline & Tính toán độ khẩn cấp (Urgency)
│   ├── ThongBao.cs             # Thông báo & Nhắc hẹn tự động
│   └── Enums.cs                # Hằng số vai trò, trạng thái pipeline, mức độ trễ hạn
│
├── Data/                       # Data Access Layer & ORM
│   ├── ComicDbContext.cs       # DbContext Entity Framework Core 8.0
│   └── DbInitializer.cs        # Tự động khởi tạo DB & Seed Data mẫu phong phú
│
├── Services/                   # Business Logic & Dịch vụ nghiệp vụ
│   ├── AppConfiguration.cs     # Đọc cấu hình từ appsettings.json
│   ├── AuthService.cs          # Đăng nhập, Ghi nhớ, Đăng ký, Quên MK OTP & Hash SHA-256
│   ├── ComicService.cs         # CRUD Truyện tranh & Chapter
│   ├── DeadlineService.cs      # Lập deadline, Cập nhật tiến độ, Nộp link Drive/PSD
│   ├── MemberService.cs        # Quản lý thành viên & Đánh giá tải công việc (Workload)
│   ├── StatisticsService.cs    # Chỉ số Dashboard, BXH thành viên, Biểu đồ tiến độ
│   └── NotificationService.cs  # Quét deadline tự động, Chuông thông báo, Toast Pop-up
│
├── Controls/                   # Custom UI Controls GDI+ phong cách Neon Studio
│   ├── StudioColors.cs         # Bảng màu chuẩn thống nhất toàn ứng dụng
│   ├── RoundedPanel.cs         # Panel bo góc mượt mà
│   ├── RoundedButton.cs        # Button bo góc tương tác phát sáng neon
│   ├── StudioProgressBar.cs    # Thanh tiến độ Cyber/Gradient
│   ├── CoverCardControl.cs     # Thẻ bìa truyện kèm thanh % tiến độ
│   ├── DeadlineItemCard.cs     # Thẻ deadline Kanban với vạch màu Xanh/Cam/Đỏ
│   └── StudioChartControl.cs   # Biểu đồ cột GDI+ neon chống răng cưa (Anti-aliasing)
│
├── Forms/                      # Giao diện người dùng chia theo module
│   ├── LoginForm.cs            # Form Đăng nhập + Tài khoản mẫu 1-click
│   ├── RegisterForm.cs         # Form Đăng ký thành viên mới
│   ├── ForgotPasswordForm.cs   # Form Khôi phục mật khẩu qua OTP & Password Strength
│   ├── MainDashboardForm.cs    # Khung giao diện chính (Sidebar, Header, Dynamic View)
│   ├── Views/                  # Các module màn hình chính:
│   │   ├── DashboardView.cs           # [1] Tổng quan chỉ số & biểu đồ studio
│   │   ├── ComicManagementView.cs     # [2] Quản lý bộ truyện & Chapters
│   │   ├── DeadlineKanbanView.cs      # [3] Quản lý Deadline 6 cột Pipeline
│   │   ├── MemberManagementView.cs    # [4] Đội ngũ thành viên & Workload
│   │   └── StatisticsView.cs          # [5] Báo cáo năng suất & BXH
│   └── Dialogs/                # Các hộp thoại nghiệp vụ:
│       ├── ComicEditDialog.cs         # Thêm / Sửa bộ truyện
│       ├── ChapterEditDialog.cs       # Thêm / Sửa chapter
│       ├── DeadlineEditDialog.cs      # Giao việc & lập hạn chót
│       ├── QuickProgressDialog.cs     # Cập nhật nhanh & nộp link Drive
│       ├── MemberEditDialog.cs        # Thêm / Sửa thành viên
│       ├── NotificationCenterDialog.cs# Trung tâm thông báo chuông
│       └── ToastNotificationForm.cs   # Pop-up khẩn cấp tự động khi login
│
├── appsettings.json            # Cấu hình ConnectionString SQL Server & Studio Settings
├── Database_Setup.sql          # Kịch bản CSDL chuẩn 3NF & Dữ liệu mẫu thực tế
└── Program.cs                  # Điểm khởi chạy ứng dụng
```

---

## 🎯 9 CHỨC NĂNG NGHIỆP VỤ BẮT BUỘC ĐÃ ĐƯỢC HIỆN THỰC

| STT | Module | Tính năng chi tiết |
|---|---|---|
| **1** | **Đăng Nhập** | Đăng nhập bằng Username / Email + Mật khẩu; Checkbox "Ghi nhớ đăng nhập"; Phân quyền Admin/Leader và Translator, Cleaner, Typesetter, QC; Cung cấp sẵn các nút chọn tài khoản mẫu tiện lợi. |
| **2** | **Đăng Ký** | Đăng ký tài khoản thành viên mới: Họ tên, Username, Email, Mật khẩu (kiểm tra độ an toàn), SĐT, Discord Tag, Chọn thế mạnh chuyên môn. |
| **3** | **Quên Mật Khẩu** | Xác thực qua Email + Sinh mã OTP 6 số bảo mật; Hộp thoại đặt lại mật khẩu có thang đo độ mạnh mật khẩu (Password Strength Meter: Yếu/Vừa/Mạnh). |
| **4** | **Quản Lý Truyện** | CRUD bộ truyện (Manga, Manhwa, Comic); Thông tin: Tên, Tác giả, Thể loại, Trạng thái, Link Folder Raw/PSD, Ảnh bìa; Quản lý danh sách các Chapter trực thuộc; Chuyển đổi giữa Chế độ lưới thẻ bìa truyện (Card Grid) và Danh sách bảng chi tiết (DataGrid Inspector). |
| **5** | **Quản Lý Deadline** | Lập deadline chi tiết từng Chapter theo từng công đoạn (Trans, Clean, Typeset, QC); Bảng Kanban Pipeline 6 cột trực quan; Trạng thái màu sắc Xanh/Cam/Đỏ; Thao tác nhanh đổi trạng thái và nộp link Drive/Dropbox. |
| **6** | **Quản Lý Thành Viên** | Danh sách nhân sự nhóm dịch; Thông tin vai trò, trạng thái (Active, Busy, On Leave); **Đánh giá tải công việc (Workload)**: Phân tích số task đang ôm, số task trễ hạn, cảnh báo quá tải. |
| **7** | **Thống Kê & Báo Cáo** | Dashboard tổng quan: Tổng truyện đang làm, Chapter xuất bản trong tháng, Deadline quá hạn; Bảng xếp hạng năng suất thành viên (Leaderboard) kèm tỷ lệ hoàn thành đúng hạn (%); Biểu đồ cột GDI+ neon hiển thị tiến độ 7 ngày gần nhất và cơ cấu công đoạn. |
| **8** | **Nhắc Hẹn & Thông Báo** | Chuông thông báo (Notification Center) ở góc giao diện kèm badge đếm số lượng chưa đọc; Tự động quét hạn chót và phát hiện task trễ hạn/sắp hết hạn; **Pop-up Toast cảnh báo khẩn cấp** tự động bung khi đăng nhập nếu có task gấp. |
| **9** | **Tìm Kiếm & Bộ Lọc** | Thanh tìm kiếm tức thì (Live Search theo thời gian thực); Bộ lọc đa tiêu chí theo thể loại truyện, trạng thái tiến độ, thành viên phụ trách và mức độ khẩn cấp deadline (An toàn, Sắp trễ, Quá hạn). |

---

## 🔑 TÀI KHOẢN MẪU ĐỂ TRẢI NGHIỆM NGAY (SEED DATA)

| Vai trò | Tên đăng nhập | Mật khẩu | Chuyên môn | Mô tả |
|---|---|---|---|---|
| **Leader / Admin** | `admin` | `admin123` | Leader/Admin | Toàn quyền quản lý Studio, duyệt xuất bản |
| **Translator** | `trans_eren` | `eren123` | Translator | Dịch thuật bản scan truyện |
| **Editor / Cleaner** | `edit_mikasa` | `mikasa123` | Cleaner/Redraw | Xóa thoại & Điền chữ (Typeset) |
| **QC Specialist** | `qc_levi` | `levi123` | QC | Kiểm duyệt chất lượng cuối cùng |

---

## 🚀 HƯỚNG DẪN KHỞI CHẠY ỨNG DỤNG

### Bước 1: Khởi chạy phần mềm
Mở terminal tại thư mục dự án và gõ lệnh:
```bash
dotnet run
```
*Hoặc mở thư mục bằng Visual Studio 2022 / JetBrains Rider và nhấn **F5**.*

### Bước 2: Cơ sở dữ liệu tự động
- Ứng dụng đã được tích hợp `DbInitializer.Initialize(db)`: Hệ thống sẽ **tự động kiểm tra và tạo CSDL `ComicManagementDb` kèm toàn bộ dữ liệu mẫu** trên Microsoft SQL Server (`.\SQLEXPRESS`) ngay khi khởi động.
- Bạn cũng có thể xem và chạy trực tiếp file script chuẩn: [Database_Setup.sql](file:///c:/Users/KIM%20THIEN/OneDrive/Desktop/ComicManagement/Database_Setup.sql).
