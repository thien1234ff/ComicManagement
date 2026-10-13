using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Services;

namespace ComicManagement.Forms
{
    public class ForgotPasswordForm : Form
    {
        private readonly AuthService _authService = new();

        private TextBox txtEmail = null!;
        private TextBox txtOtp = null!;
        private TextBox txtNewPassword = null!;
        private TextBox txtConfirmPassword = null!;
        private StudioProgressBar barStrength = null!;
        private Label lblStrengthText = null!;
        private Label lblStatus = null!;
        private RoundedButton btnSendOtp = null!;
        private RoundedButton btnResetPass = null!;

        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Manga Editorial Studio Hub - Khôi Phục Mật Khẩu";
            Size = new Size(500, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(440, 540),
                Location = new Point(22, 20),
                BorderRadius = 18,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = "KHÔI PHỤC MẬT KHẨU",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(20, 16),
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            var lblSubtitle = new Label
            {
                Text = "Xác thực qua Email Studio + Mã OTP bảo mật 6 số",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(20, 46),
                Size = new Size(400, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            int currentY = 75;

            // 1. Email + Button Gửi OTP
            AddLabel("Email đăng ký tài khoản *", card, 32, currentY);
            currentY += 22;

            var pnlEmail = new RoundedPanel
            {
                Location = new Point(32, currentY),
                Size = new Size(240, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 8, 10, 8),
                Parent = card
            };

            txtEmail = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                Text = "leader@comicstudio.vn", // Gợi ý sẵn email mẫu
                Parent = pnlEmail
            };

            btnSendOtp = new RoundedButton
            {
                Text = "Gửi mã OTP",
                Location = new Point(282, currentY),
                Size = new Size(125, 38),
                BorderRadius = 8,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = card
            };
            btnSendOtp.Click += BtnSendOtp_Click;
            currentY += 52;

            // 2. Mã OTP 6 chữ số
            AddLabel("Mã OTP xác thực (6 chữ số) *", card, 32, currentY);
            currentY += 22;
            txtOtp = AddInput(card, 32, currentY);
            currentY += 52;

            // 3. Mật khẩu mới
            AddLabel("Mật khẩu mới *", card, 32, currentY);
            currentY += 22;
            txtNewPassword = AddInput(card, 32, currentY, isPassword: true);
            txtNewPassword.TextChanged += TxtNewPassword_TextChanged;
            currentY += 46;

            // Password Strength Meter
            barStrength = new StudioProgressBar
            {
                Location = new Point(32, currentY),
                Size = new Size(250, 10),
                BorderRadius = 4,
                Value = 0,
                ShowPercentageText = false,
                Parent = card
            };

            lblStrengthText = new Label
            {
                Text = "Độ mạnh: Chưa nhập",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(290, currentY - 4),
                Size = new Size(120, 18),
                Parent = card
            };
            currentY += 20;

            // 4. Nhập lại mật khẩu mới
            AddLabel("Xác nhận mật khẩu mới *", card, 32, currentY);
            currentY += 22;
            txtConfirmPassword = AddInput(card, 32, currentY, isPassword: true);
            currentY += 48;

            // Status message
            lblStatus = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.SafeGreen,
                Location = new Point(32, currentY),
                Size = new Size(375, 34),
                TextAlign = ContentAlignment.MiddleLeft,
                Parent = card
            };
            currentY += 36;

            // Action buttons
            btnResetPass = new RoundedButton
            {
                Text = "ĐẶT LẠI MẬT KHẨU",
                Location = new Point(32, currentY),
                Size = new Size(375, 42),
                BorderRadius = 10,
                NormalColor = StudioColors.CyanNeon,
                HoverColor = Color.FromArgb(34, 211, 238),
                Parent = card
            };
            btnResetPass.Click += BtnResetPass_Click;
            currentY += 48;

            var btnClose = new RoundedButton
            {
                Text = "Quay lại",
                Location = new Point(32, currentY),
                Size = new Size(375, 32),
                BorderRadius = 8,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnClose.Click += (s, e) => Close();
        }

        private void AddLabel(string text, Control parent, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(x, y),
                Size = new Size(375, 20),
                Parent = parent
            };
        }

        private TextBox AddInput(Control parent, int x, int y, bool isPassword = false)
        {
            var pnl = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(375, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 8, 10, 8),
                Parent = parent
            };

            var txt = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                PasswordChar = isPassword ? '●' : '\0',
                Parent = pnl
            };
            return txt;
        }

        private void TxtNewPassword_TextChanged(object? sender, EventArgs e)
        {
            int score = AuthService.EvaluatePasswordStrength(txtNewPassword.Text);
            barStrength.Value = score;

            if (score < 40)
            {
                barStrength.StartColor = StudioColors.DangerRed;
                barStrength.EndColor = StudioColors.DangerRed;
                lblStrengthText.ForeColor = StudioColors.DangerRed;
                lblStrengthText.Text = "Độ mạnh: Yếu";
            }
            else if (score < 75)
            {
                barStrength.StartColor = StudioColors.WarningAmber;
                barStrength.EndColor = StudioColors.WarningAmber;
                lblStrengthText.ForeColor = StudioColors.WarningAmber;
                lblStrengthText.Text = "Độ mạnh: Vừa";
            }
            else
            {
                barStrength.StartColor = StudioColors.SafeGreen;
                barStrength.EndColor = StudioColors.CyanNeon;
                lblStrengthText.ForeColor = StudioColors.SafeGreen;
                lblStrengthText.Text = "Độ mạnh: Rất mạnh";
            }
        }

        private void BtnSendOtp_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";
            string email = txtEmail.Text.Trim();
            var res = _authService.GeneratePasswordResetOtp(email);

            if (!res.Success)
            {
                lblStatus.ForeColor = StudioColors.DangerRed;
                lblStatus.Text = res.Message;
                return;
            }

            lblStatus.ForeColor = StudioColors.SafeGreen;
            lblStatus.Text = res.Message;
            txtOtp.Text = res.GeneratedOtp ?? ""; // Tự động điền OTP hỗ trợ trải nghiệm demo mượt mà!

            MessageBox.Show(
                $"Mã OTP bảo mật được cấp cho email '{email}' là: {res.GeneratedOtp}\n\n(Hệ thống đã tự động điền vào ô OTP cho bạn)",
                "Mã Xác Thực OTP",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnResetPass_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";

            string email = txtEmail.Text.Trim();
            string otp = txtOtp.Text.Trim();
            string newPass = txtNewPassword.Text;
            string confirm = txtConfirmPassword.Text;

            if (newPass != confirm)
            {
                lblStatus.ForeColor = StudioColors.DangerRed;
                lblStatus.Text = "Mật khẩu xác nhận không trùng khớp!";
                return;
            }

            var res = _authService.VerifyOtpAndResetPassword(email, otp, newPass);
            if (!res.Success)
            {
                lblStatus.ForeColor = StudioColors.DangerRed;
                lblStatus.Text = res.Message;
                return;
            }

            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
