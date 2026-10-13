using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ComicManagement.Data;
using ComicManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicManagement.Services
{
    public class AuthService
    {
        public static ThanhVien? CurrentUser { get; private set; }

        private static readonly string RememberFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ComicManagement_session.txt"
        );

        // Giả lập cache OTP: Email -> (OTP, ExpiryTime)
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Otp, DateTime Expiry)> OtpCache = new();

        public (bool Success, string Message, ThanhVien? User) Login(string usernameOrEmail, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!", null);
            }

            using var db = new ComicDbContext();
            var user = db.ThanhViens
                .Include(u => u.CacCongDoan)
                .FirstOrDefault(u => u.TenDangNhap == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
            {
                return (false, "Tài khoản không tồn tại trên hệ thống!", null);
            }

            // Kiểm tra mật khẩu (hỗ trợ cả plain text seed và hash)
            if (user.MatKhauHash != password && user.MatKhauHash != HashPassword(password))
            {
                return (false, "Mật khẩu không chính xác!", null);
            }

            CurrentUser = user;

            if (rememberMe)
            {
                SaveRememberMe(user.TenDangNhap);
            }
            else
            {
                ClearRememberMe();
            }

            return (true, "Đăng nhập thành công!", user);
        }

        public (bool Success, string Message, ThanhVien? User) Register(
            string hoTen,
            string tenDangNhap,
            string email,
            string password,
            string? soDienThoai,
            string? discordTag,
            string vaiTroChuyenMon)
        {
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(tenDangNhap) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Vui lòng điền đầy đủ các thông tin bắt buộc!", null);
            }

            if (password.Length < 6)
            {
                return (false, "Mật khẩu phải có ít nhất 6 ký tự!", null);
            }

            using var db = new ComicDbContext();
            if (db.ThanhViens.Any(u => u.TenDangNhap == tenDangNhap))
            {
                return (false, "Tên đăng nhập này đã được sử dụng!", null);
            }

            if (db.ThanhViens.Any(u => u.Email == email))
            {
                return (false, "Email này đã được đăng ký!", null);
            }

            var newUser = new ThanhVien
            {
                HoTen = hoTen.Trim(),
                TenDangNhap = tenDangNhap.Trim(),
                MatKhauHash = HashPassword(password),
                Email = email.Trim(),
                SoDienThoai = soDienThoai?.Trim(),
                DiscordTag = discordTag?.Trim(),
                VaiTroChuyenMon = string.IsNullOrWhiteSpace(vaiTroChuyenMon) ? StudioRoles.Translator : vaiTroChuyenMon,
                TrangThaiHoatDong = MemberStatuses.Active,
                NgayTao = DateTime.Now
            };

            db.ThanhViens.Add(newUser);
            db.SaveChanges();

            return (true, "Đăng ký tài khoản thành công!", newUser);
        }

        public (bool Success, string Message, string? GeneratedOtp) GeneratePasswordResetOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Vui lòng nhập địa chỉ email!", null);
            }

            using var db = new ComicDbContext();
            var user = db.ThanhViens.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return (false, "Email không tồn tại trong hệ thống!", null);
            }

            // Sinh mã OTP 6 chữ số
            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            DateTime expiry = DateTime.Now.AddMinutes(5);

            OtpCache[email.ToLower()] = (otp, expiry);

            return (true, $"Mã xác thực OTP đã được tạo: {otp} (Hiệu lực trong 5 phút)", otp);
        }

        public (bool Success, string Message) VerifyOtpAndResetPassword(string email, string otp, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
            {
                return (false, "Vui lòng điền đầy đủ email, OTP và mật khẩu mới!");
            }

            if (newPassword.Length < 6)
            {
                return (false, "Mật khẩu mới phải có tối thiểu 6 ký tự!");
            }

            if (!OtpCache.TryGetValue(email.ToLower(), out var cached) || cached.Otp != otp)
            {
                return (false, "Mã OTP không chính xác hoặc chưa được tạo!");
            }

            if (DateTime.Now > cached.Expiry)
            {
                return (false, "Mã OTP đã hết hạn! Vui lòng yêu cầu mã mới.");
            }

            using var db = new ComicDbContext();
            var user = db.ThanhViens.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return (false, "Không tìm thấy người dùng!");
            }

            user.MatKhauHash = HashPassword(newPassword);
            db.SaveChanges();

            OtpCache.TryRemove(email.ToLower(), out _);

            return (true, "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay.");
        }

        public static int EvaluatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;
            int score = 0;
            if (password.Length >= 6) score += 25;
            if (password.Length >= 10) score += 25;
            if (password.Any(char.IsDigit)) score += 20;
            if (password.Any(char.IsUpper)) score += 15;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 15;
            return Math.Min(100, score);
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public string? GetRememberedUsername()
        {
            try
            {
                if (File.Exists(RememberFilePath))
                {
                    return File.ReadAllText(RememberFilePath).Trim();
                }
            }
            catch { }
            return null;
        }

        private void SaveRememberMe(string username)
        {
            try
            {
                File.WriteAllText(RememberFilePath, username);
            }
            catch { }
        }

        private void ClearRememberMe()
        {
            try
            {
                if (File.Exists(RememberFilePath))
                {
                    File.Delete(RememberFilePath);
                }
            }
            catch { }
        }

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
