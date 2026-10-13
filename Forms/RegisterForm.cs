using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms
{
    public class RegisterForm : Form
    {
        private readonly AuthService _authService = new();

        public string RegisteredUsername { get; private set; } = string.Empty;

        private TextBox txtHoTen = null!;
        private TextBox txtUsername = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private TextBox txtPhone = null!;
        private TextBox txtDiscord = null!;
        private ComboBox cboRole = null!;
        private Label lblError = null!;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Manga Editorial Studio Hub - Đăng Ký Thành Viên";
            Size = new Size(540, 720);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(480, 640),
                Location = new Point(22, 20),
                BorderRadius = 18,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = "ĐĂNG KÝ THÀNH VIÊN STUDIO",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = StudioColors.SecondaryPink,
                Location = new Point(20, 16),
                Size = new Size(440, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            var lblSubtitle = new Label
            {
                Text = "Đăng ký tài khoản thành viên đội ngũ dịch & biên tập",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextMuted,
                Location = new Point(20, 46),
                Size = new Size(440, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            int currentY = 75;

            // 1. Họ và tên
            AddLabel("Họ và tên *", card, 36, currentY);
            txtHoTen = AddInput(card, 36, currentY + 22);
            currentY += 68;

            // 2. Tên đăng nhập & Email (2 cột)
            AddLabel("Tên đăng nhập *", card, 36, currentY, 190);
            AddLabel("Email *", card, 240, currentY, 190);
            txtUsername = AddInput(card, 36, currentY + 22, 190);
            txtEmail = AddInput(card, 240, currentY + 22, 190);
            currentY += 68;

            // 3. Mật khẩu
            AddLabel("Mật khẩu (tối thiểu 6 ký tự) *", card, 36, currentY);
            txtPassword = AddInput(card, 36, currentY + 22, isPassword: true);
            currentY += 68;

            // 4. Số điện thoại & Discord Tag (2 cột)
            AddLabel("Số điện thoại", card, 36, currentY, 190);
            AddLabel("Discord Tag (vd: User#1234)", card, 240, currentY, 190);
            txtPhone = AddInput(card, 36, currentY + 22, 190);
            txtDiscord = AddInput(card, 240, currentY + 22, 190);
            currentY += 68;

            // 5. Thế mạnh / Vai trò chuyên môn
            AddLabel("Vai trò / Thế mạnh chuyên môn *", card, 36, currentY);
            var pnlRole = new RoundedPanel
            {
                Location = new Point(36, currentY + 22),
                Size = new Size(394, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6, 6, 6, 6),
                Parent = card
            };

            cboRole = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Parent = pnlRole
            };
            cboRole.Items.AddRange(new object[]
            {
                StudioRoles.Translator,
                StudioRoles.CleanerRedraw,
                StudioRoles.Typesetter,
                StudioRoles.QC
            });
            cboRole.SelectedIndex = 0;
            currentY += 70;

            // Error message
            lblError = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.DangerRed,
                Location = new Point(36, currentY),
                Size = new Size(394, 24),
                TextAlign = ContentAlignment.MiddleLeft,
                Parent = card
            };
            currentY += 28;

            // Register button
            var btnRegister = new RoundedButton
            {
                Text = "HOÀN TẤT ĐĂNG KÝ",
                Location = new Point(36, currentY),
                Size = new Size(394, 42),
                BorderRadius = 10,
                NormalColor = StudioColors.SecondaryPink,
                HoverColor = Color.FromArgb(244, 114, 182),
                Parent = card
            };
            btnRegister.Click += BtnRegister_Click;
            currentY += 50;

            var btnCancel = new RoundedButton
            {
                Text = "Quay lại Đăng nhập",
                Location = new Point(36, currentY),
                Size = new Size(394, 34),
                BorderRadius = 8,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnCancel.Click += (s, e) => Close();
        }

        private void AddLabel(string text, Control parent, int x, int y, int width = 394)
        {
            var lbl = new Label
            {
                Text = text,
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.TextSecondary,
                Location = new Point(x, y),
                Size = new Size(width, 20),
                Parent = parent
            };
        }

        private TextBox AddInput(Control parent, int x, int y, int width = 394, bool isPassword = false)
        {
            var pnl = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(width, 38),
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

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";

            string hoTen = txtHoTen.Text.Trim();
            string user = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text;
            string phone = txtPhone.Text.Trim();
            string discord = txtDiscord.Text.Trim();
            string role = cboRole.SelectedItem?.ToString() ?? StudioRoles.Translator;

            var res = _authService.Register(hoTen, user, email, pass, phone, discord, role);
            if (!res.Success)
            {
                lblError.ForeColor = StudioColors.DangerRed;
                lblError.Text = res.Message;
                return;
            }

            RegisteredUsername = user;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
