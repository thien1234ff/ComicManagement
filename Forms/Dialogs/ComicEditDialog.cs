using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Dialogs
{
    public class ComicEditDialog : Form
    {
        private readonly ComicService _comicService = new();
        private readonly Truyen? _existingComic;

        private TextBox txtTenTruyen = null!;
        private TextBox txtTacGia = null!;
        private TextBox txtTheLoai = null!;
        private ComboBox cboLoaiTruyen = null!;
        private ComboBox cboTrangThai = null!;
        private TextBox txtFolderRaw = null!;
        private TextBox txtHinhAnhBia = null!;
        private TextBox txtMoTa = null!;
        private Label lblError = null!;

        public ComicEditDialog(Truyen? comic = null)
        {
            _existingComic = comic;
            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            Text = _existingComic == null ? "Thêm Bộ Truyện Mới" : $"Chỉnh Sửa: {_existingComic.TenTruyen}";
            Size = new Size(580, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(520, 600),
                Location = new Point(22, 18),
                BorderRadius = 16,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = _existingComic == null ? "THÊM BỘ TRUYỆN MỚI" : "CẬP NHẬT THÔNG TIN TRUYỆN",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = StudioColors.PrimaryPurple,
                Location = new Point(20, 16),
                Size = new Size(480, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            int currentY = 54;

            // 1. Tên truyện
            AddLabel("Tên bộ truyện (Manga/Manhwa/Comic) *", card, 28, currentY);
            txtTenTruyen = AddInput(card, 28, currentY + 22, 460);
            currentY += 66;

            // 2. Tác giả & Loại truyện
            AddLabel("Tác giả / Họa sĩ *", card, 28, currentY, 220);
            AddLabel("Loại truyện *", card, 268, currentY, 220);
            txtTacGia = AddInput(card, 28, currentY + 22, 220);

            var pnlLoai = new RoundedPanel
            {
                Location = new Point(268, currentY + 22),
                Size = new Size(220, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboLoaiTruyen = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlLoai
            };
            cboLoaiTruyen.Items.AddRange(new object[] { "Manga", "Manhwa", "Manhua", "Comic" });
            cboLoaiTruyen.SelectedIndex = 0;
            currentY += 66;

            // 3. Thể loại & Trạng thái
            AddLabel("Thể loại (Action, Shounen, Fantasy,...) *", card, 28, currentY, 220);
            AddLabel("Trạng thái tiến độ *", card, 268, currentY, 220);
            txtTheLoai = AddInput(card, 28, currentY + 22, 220);

            var pnlStatus = new RoundedPanel
            {
                Location = new Point(268, currentY + 22),
                Size = new Size(220, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(6),
                Parent = card
            };
            cboTrangThai = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlStatus
            };
            cboTrangThai.Items.AddRange(new object[] { ComicStatuses.InProgress, ComicStatuses.OnHold, ComicStatuses.Completed });
            cboTrangThai.SelectedIndex = 0;
            currentY += 66;

            // 4. Folder Raw/PSD & Ảnh bìa
            AddLabel("Đường dẫn Folder Raw / PSD lưu trữ", card, 28, currentY);
            txtFolderRaw = AddInput(card, 28, currentY + 22, 460);
            currentY += 66;

            AddLabel("Link Ảnh bìa truyện (URL hoặc đường dẫn)", card, 28, currentY);
            txtHinhAnhBia = AddInput(card, 28, currentY + 22, 460);
            currentY += 66;

            // 5. Mô tả
            AddLabel("Tóm tắt nội dung truyện", card, 28, currentY);
            var pnlMoTa = new RoundedPanel
            {
                Location = new Point(28, currentY + 22),
                Size = new Size(460, 60),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(8),
                Parent = card
            };
            txtMoTa = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                Parent = pnlMoTa
            };
            currentY += 90;

            lblError = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.DangerRed,
                Location = new Point(28, currentY),
                Size = new Size(460, 20),
                Parent = card
            };
            currentY += 24;

            // Action Buttons
            var btnSave = new RoundedButton
            {
                Text = _existingComic == null ? "LƯU TRUYỆN MỚI" : "LƯU THAY ĐỔI",
                Location = new Point(28, currentY),
                Size = new Size(225, 40),
                BorderRadius = 8,
                NormalColor = StudioColors.PrimaryPurple,
                HoverColor = StudioColors.PrimaryPurpleHover,
                Parent = card
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new RoundedButton
            {
                Text = "Hủy bỏ",
                Location = new Point(263, currentY),
                Size = new Size(225, 40),
                BorderRadius = 8,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnCancel.Click += (s, e) => Close();
        }

        private void AddLabel(string text, Control parent, int x, int y, int width = 460)
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

        private TextBox AddInput(Control parent, int x, int y, int width = 460)
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
                Parent = pnl
            };
            return txt;
        }

        private void PopulateData()
        {
            if (_existingComic == null) return;
            txtTenTruyen.Text = _existingComic.TenTruyen;
            txtTacGia.Text = _existingComic.TacGia;
            txtTheLoai.Text = _existingComic.TheLoai;
            cboLoaiTruyen.SelectedItem = _existingComic.LoaiTruyen;
            cboTrangThai.SelectedItem = _existingComic.TrangThai;
            txtFolderRaw.Text = _existingComic.FolderRawPsd ?? "";
            txtHinhAnhBia.Text = _existingComic.HinhAnhBia ?? "";
            txtMoTa.Text = _existingComic.MoTa ?? "";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            string ten = txtTenTruyen.Text.Trim();
            string tacGia = txtTacGia.Text.Trim();
            string theLoai = txtTheLoai.Text.Trim();

            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(tacGia) || string.IsNullOrWhiteSpace(theLoai))
            {
                lblError.Text = "Vui lòng nhập Tên truyện, Tác giả và Thể loại!";
                return;
            }

            if (_existingComic == null)
            {
                var newComic = new Truyen
                {
                    TenTruyen = ten,
                    TacGia = tacGia,
                    TheLoai = theLoai,
                    LoaiTruyen = cboLoaiTruyen.SelectedItem?.ToString() ?? "Manga",
                    TrangThai = cboTrangThai.SelectedItem?.ToString() ?? ComicStatuses.InProgress,
                    FolderRawPsd = txtFolderRaw.Text.Trim(),
                    HinhAnhBia = txtHinhAnhBia.Text.Trim(),
                    MoTa = txtMoTa.Text.Trim()
                };

                var res = _comicService.AddComic(newComic);
                if (!res.Success)
                {
                    lblError.Text = res.Message;
                    return;
                }
            }
            else
            {
                _existingComic.TenTruyen = ten;
                _existingComic.TacGia = tacGia;
                _existingComic.TheLoai = theLoai;
                _existingComic.LoaiTruyen = cboLoaiTruyen.SelectedItem?.ToString() ?? "Manga";
                _existingComic.TrangThai = cboTrangThai.SelectedItem?.ToString() ?? ComicStatuses.InProgress;
                _existingComic.FolderRawPsd = txtFolderRaw.Text.Trim();
                _existingComic.HinhAnhBia = txtHinhAnhBia.Text.Trim();
                _existingComic.MoTa = txtMoTa.Text.Trim();

                var res = _comicService.UpdateComic(_existingComic);
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
