using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Services;

namespace ComicManagement.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService = new();
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private CheckBox chkRemember = null!;
        private Label lblError = null!;
        private RoundedButton btnLogin = null!;

        public LoginForm()
        {
            InitializeComponent();
            LoadRememberedCredentials();
        }

        private void InitializeComponent()
        {
            Text = "Manga Editorial Studio Hub - Đăng Nhập";
            Size = new Size(480, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(420, 540),
                Location = new Point(22, 18),
                BorderRadius = 12,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            // Title
            var lblLogo = new Label
            {
                Text = "COMIC STUDIO HUB",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = StudioColors.PrimaryPurple,
                Location = new Point(20, 20),
                Size = new Size(380, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            var lblSubtitle = new Label
            {
                Text = "Hệ thống quản lý biên tập truyện tranh",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(20, 52),
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            // Username
            var lblUser = new Label
            {
                Text = "Tên đăng nhập hoặc Email",
                Font = StudioColors.SubtitleFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(30, 95),
                Size = new Size(360, 20),
                Parent = card
            };

            var pnlUser = new RoundedPanel
            {
                Location = new Point(30, 118),
                Size = new Size(360, 40),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 8, 10, 8),
                Parent = card
            };

            txtUsername = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                Parent = pnlUser
            };

            // Password
            var lblPass = new Label
            {
                Text = "Mật khẩu",
                Font = StudioColors.SubtitleFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(30, 170),
                Size = new Size(360, 20),
                Parent = card
            };

            var pnlPass = new RoundedPanel
            {
                Location = new Point(30, 193),
                Size = new Size(360, 40),
                BorderRadius = 6,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(10, 8, 10, 8),
                Parent = card
            };

            txtPassword = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                PasswordChar = '●',
                Font = new Font("Segoe UI", 10f),
                Parent = pnlPass
            };

            // Remember & Forgot
            chkRemember = new CheckBox
            {
                Text = "Ghi nhớ đăng nhập",
                Font = StudioColors.RegularFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(30, 245),
                Size = new Size(180, 24),
                Checked = true,
                Parent = card
            };

            var lnkForgot = new LinkLabel
            {
                Text = "Quên mật khẩu?",
                Font = StudioColors.RegularFont,
                LinkColor = StudioColors.CyanNeon,
                ActiveLinkColor = StudioColors.SecondaryPink,
                Location = new Point(250, 245),
                Size = new Size(140, 24),
                TextAlign = ContentAlignment.TopRight,
                Parent = card
            };
            lnkForgot.LinkClicked += (s, e) =>
            {
                var forgotForm = new ForgotPasswordForm();
                forgotForm.ShowDialog(this);
            };

            // Error
            lblError = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.DangerRed,
                Location = new Point(30, 272),
                Size = new Size(360, 20),
                Parent = card
            };

            // Login Button
            btnLogin = new RoundedButton
            {
                Text = "ĐĂNG NHẬP",
                Location = new Point(30, 298),
                Size = new Size(360, 42),
                BorderRadius = 6,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = card
            };
            btnLogin.Click += BtnLogin_Click;

            // Register Link
            var pnlRegister = new FlowLayoutPanel
            {
                Location = new Point(30, 350),
                Size = new Size(360, 26),
                FlowDirection = FlowDirection.LeftToRight,
                Parent = card
            };

            var lblNoAccount = new Label
            {
                Text = "Chưa có tài khoản? ",
                Font = StudioColors.RegularFont,
                ForeColor = StudioColors.TextMuted,
                AutoSize = true,
                Parent = pnlRegister
            };

            var lnkRegister = new LinkLabel
            {
                Text = "Đăng ký thành viên",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                LinkColor = StudioColors.SecondaryPink,
                ActiveLinkColor = Color.White,
                AutoSize = true,
                Parent = pnlRegister
            };
            lnkRegister.LinkClicked += (s, e) =>
            {
                var regForm = new RegisterForm();
                if (regForm.ShowDialog(this) == DialogResult.OK)
                {
                    txtUsername.Text = regForm.RegisteredUsername;
                    txtPassword.Text = "";
                    lblError.ForeColor = StudioColors.SafeGreen;
                    lblError.Text = "Đăng ký thành công! Vui lòng đăng nhập.";
                }
            };

            // Demo Accounts Panel
            var pnlDemo = new RoundedPanel
            {
                Location = new Point(30, 395),
                Size = new Size(360, 115),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = Color.FromArgb(50, 50, 75),
                Padding = new Padding(8),
                Parent = card
            };

            var lblDemoTitle = new Label
            {
                Text = "Tài khoản mẫu trải nghiệm nhanh:",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(8, 8),
                Size = new Size(340, 18),
                Parent = pnlDemo
            };

            var btnDemoAdmin = new RoundedButton
            {
                Text = "Leader (admin)",
                Location = new Point(8, 32),
                Size = new Size(165, 30),
                BorderRadius = 4,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.PrimaryPurple,
                Parent = pnlDemo
            };
            btnDemoAdmin.Click += (s, e) => { txtUsername.Text = "admin"; txtPassword.Text = "admin123"; };

            var btnDemoTrans = new RoundedButton
            {
                Text = "Translator (eren)",
                Location = new Point(185, 32),
                Size = new Size(165, 30),
                BorderRadius = 4,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.CyanNeon,
                Parent = pnlDemo
            };
            btnDemoTrans.Click += (s, e) => { txtUsername.Text = "trans_eren"; txtPassword.Text = "eren123"; };

            var btnDemoEdit = new RoundedButton
            {
                Text = "Cleaner (mikasa)",
                Location = new Point(8, 70),
                Size = new Size(165, 30),
                BorderRadius = 4,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.SecondaryPink,
                Parent = pnlDemo
            };
            btnDemoEdit.Click += (s, e) => { txtUsername.Text = "edit_mikasa"; txtPassword.Text = "mikasa123"; };

            var btnDemoQC = new RoundedButton
            {
                Text = "QC Duyệt (levi)",
                Location = new Point(185, 70),
                Size = new Size(165, 30),
                BorderRadius = 4,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = StudioColors.SafeGreen,
                Parent = pnlDemo
            };
            btnDemoQC.Click += (s, e) => { txtUsername.Text = "qc_levi"; txtPassword.Text = "levi123"; };

            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
            txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
        }

        private void LoadRememberedCredentials()
        {
            string? remembered = _authService.GetRememberedUsername();
            if (!string.IsNullOrWhiteSpace(remembered))
            {
                txtUsername.Text = remembered;
                chkRemember.Checked = true;
                txtPassword.Focus();
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            var res = _authService.Login(txtUsername.Text.Trim(), txtPassword.Text, chkRemember.Checked);
            if (!res.Success)
            {
                lblError.ForeColor = StudioColors.DangerRed;
                lblError.Text = res.Message;
                return;
            }

            Hide();
            var mainForm = new MainDashboardForm();
            mainForm.FormClosed += (s, args) => Close();
            mainForm.Show();
        }
    }
}
