using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Dialogs
{
    public class MemberEditDialog : Form
    {
        private readonly MemberService _memberService = new();
        private readonly ThanhVien? _existingMember;

        private TextBox txtHoTen = null!;
        private TextBox txtUsername = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private TextBox txtPhone = null!;
        private TextBox txtDiscord = null!;
        private ComboBox cboRole = null!;
        private ComboBox cboStatus = null!;
        private Label lblError = null!;

        public MemberEditDialog(ThanhVien? member = null)
        {
            _existingMember = member;
            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            Text = _existingMember == null ? "Thêm Thành Viên Mới" : $"Chỉnh Sửa Thành Viên: {_existingMember.HoTen}";
            Size = new Size(540, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(480, 600),
                Location = new Point(22, 18),
                BorderRadius = 16,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = _existingMember == null ? "THÊM THÀNH VIÊN STUDIO" : "CẬP NHẬT THÀNH VIÊN",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = StudioColors.SecondaryPink,
                Location = new Point(20, 16),
                Size = new Size(440, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            int currentY = 52;

            // 1. Họ và tên
            AddLabel("Họ và tên *", card, 28, currentY);
            txtHoTen = AddInput(card, 28, currentY + 22, 420);
            currentY += 66;

            // 2. Tên đăng nhập & Email
            AddLabel("Tên đăng nhập *", card, 28, currentY, 200);
            AddLabel("Email *", card, 248, currentY, 200);
            txtUsername = AddInput(card, 28, currentY + 22, 200);
            txtEmail = AddInput(card, 248, currentY + 22, 200);
            if (_existingMember != null) txtUsername.ReadOnly = true;
            currentY += 66;

            // 3. Mật khẩu
            string passLabel = _existingMember == null ? "Mật khẩu ban đầu *" : "Đổi mật khẩu mới (để trống nếu không đổi)";
            AddLabel(passLabel, card, 28, currentY);
            txtPassword = AddInput(card, 28, currentY + 22, 420, isPassword: true);
            currentY += 66;

            // 4. Số điện thoại & Discord
            AddLabel("Số điện thoại", card, 28, currentY, 200);
            AddLabel("Discord Tag (vd: Levi#9999)", card, 248, currentY, 200);
            txtPhone = AddInput(card, 28, currentY + 22, 200);
            txtDiscord = AddInput(card, 248, currentY + 22, 200);
            currentY += 66;

            // 5. Vai trò & Trạng thái hoạt động
            AddLabel("Vai trò chuyên môn *", card, 28, currentY, 200);
            AddLabel("Trạng thái hoạt động *", card, 248, currentY, 200);

            var pnlRole = new RoundedPanel
            {
                Location = new Point(28, currentY + 22),
                Size = new Size(200, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboRole = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlRole
            };
            cboRole.Items.AddRange(StudioRoles.AllRoles);
            cboRole.SelectedIndex = 1;

            var pnlStatus = new RoundedPanel
            {
                Location = new Point(248, currentY + 22),
                Size = new Size(200, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboStatus = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlStatus
            };
            cboStatus.Items.AddRange(MemberStatuses.All);
            cboStatus.SelectedIndex = 0;
            currentY += 70;

            lblError = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.DangerRed,
                Location = new Point(28, currentY),
                Size = new Size(420, 20),
                Parent = card
            };
            currentY += 24;

            // Buttons
            var btnSave = new RoundedButton
            {
                Text = _existingMember == null ? "LƯU THÀNH VIÊN" : "LƯU THAY ĐỔI",
                Location = new Point(28, currentY),
                Size = new Size(205, 40),
                BorderRadius = 8,
                NormalColor = StudioColors.SecondaryPink,
                HoverColor = Color.FromArgb(244, 114, 182),
                Parent = card
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new RoundedButton
            {
                Text = "Hủy bỏ",
                Location = new Point(243, currentY),
                Size = new Size(205, 40),
                BorderRadius = 8,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnCancel.Click += (s, e) => Close();
        }

        private void AddLabel(string text, Control parent, int x, int y, int width = 420)
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

        private TextBox AddInput(Control parent, int x, int y, int width = 420, bool isPassword = false)
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

        private void PopulateData()
        {
            if (_existingMember == null) return;
            txtHoTen.Text = _existingMember.HoTen;
            txtUsername.Text = _existingMember.TenDangNhap;
            txtEmail.Text = _existingMember.Email;
            txtPhone.Text = _existingMember.SoDienThoai ?? "";
            txtDiscord.Text = _existingMember.DiscordTag ?? "";
            cboRole.SelectedItem = _existingMember.VaiTroChuyenMon;
            cboStatus.SelectedItem = _existingMember.TrangThaiHoatDong;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            string hoTen = txtHoTen.Text.Trim();
            string user = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string role = cboRole.SelectedItem?.ToString() ?? StudioRoles.Translator;
            string status = cboStatus.SelectedItem?.ToString() ?? MemberStatuses.Active;

            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(email))
            {
                lblError.Text = "Vui lòng nhập Họ tên, Tên đăng nhập và Email!";
                return;
            }

            if (_existingMember == null)
            {
                var newMem = new ThanhVien
                {
                    HoTen = hoTen,
                    TenDangNhap = user,
                    Email = email,
                    SoDienThoai = txtPhone.Text.Trim(),
                    DiscordTag = txtDiscord.Text.Trim(),
                    VaiTroChuyenMon = role,
                    TrangThaiHoatDong = status
                };

                var res = _memberService.AddMember(newMem, txtPassword.Text);
                if (!res.Success)
                {
                    lblError.Text = res.Message;
                    return;
                }
            }
            else
            {
                _existingMember.HoTen = hoTen;
                _existingMember.Email = email;
                _existingMember.SoDienThoai = txtPhone.Text.Trim();
                _existingMember.DiscordTag = txtDiscord.Text.Trim();
                _existingMember.VaiTroChuyenMon = role;
                _existingMember.TrangThaiHoatDong = status;

                var res = _memberService.UpdateMember(_existingMember, string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text);
                if (!res.Success)
                {
                    lblError.Text = res.Message;
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
