using System;
using System.Drawing;
using System.Windows.Forms;
using ComicManagement.Controls;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Forms.Dialogs
{
    public class ChapterEditDialog : Form
    {
        private readonly ComicService _comicService = new();
        private readonly int _truyenId;
        private readonly Chapter? _existingChapter;

        private NumericUpDown numSoChapter = null!;
        private TextBox txtTieuDe = null!;
        private ComboBox cboTrangThai = null!;
        private TextBox txtFolderPsd = null!;
        private CheckBox chkAutoPipeline = null!;
        private Label lblError = null!;

        public ChapterEditDialog(int truyenId, Chapter? chapter = null)
        {
            _truyenId = truyenId;
            _existingChapter = chapter;
            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            Text = _existingChapter == null ? "Thêm Chapter Mới" : $"Chỉnh Sửa Chapter {_existingChapter.SoChapter}";
            Size = new Size(500, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = StudioColors.BackgroundDark;
            ForeColor = StudioColors.TextWhite;

            var card = new RoundedPanel
            {
                Size = new Size(440, 400),
                Location = new Point(22, 18),
                BorderRadius = 14,
                CustomBackgroundColor = StudioColors.SidebarDark,
                BorderColor = StudioColors.BorderColor,
                BorderSize = 1,
                Parent = this
            };

            var lblTitle = new Label
            {
                Text = _existingChapter == null ? "THÊM CHAPTER MỚI" : "CẬP NHẬT CHAPTER",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = StudioColors.CyanNeon,
                Location = new Point(20, 16),
                Size = new Size(400, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = card
            };

            int currentY = 54;

            // 1. Số Chapter & Trạng thái
            AddLabel("Số Chapter *", card, 28, currentY, 180);
            AddLabel("Trạng thái chapter *", card, 228, currentY, 180);

            var pnlNum = new RoundedPanel
            {
                Location = new Point(28, currentY + 22),
                Size = new Size(180, 38),
                BorderRadius = 8,
                CustomBackgroundColor = StudioColors.CardDark,
                BorderColor = StudioColors.BorderColor,
                Padding = new Padding(8),
                Parent = card
            };
            numSoChapter = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                BackColor = StudioColors.CardDark,
                ForeColor = StudioColors.TextWhite,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                DecimalPlaces = 1,
                Maximum = 9999,
                Minimum = 0,
                Value = 1,
                Parent = pnlNum
            };

            var pnlStatus = new RoundedPanel
            {
                Location = new Point(228, currentY + 22),
                Size = new Size(180, 38),
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
            cboTrangThai.Items.AddRange(new object[] { "Chưa bắt đầu", "Đang biên tập", "Đã hoàn tất", "Đã xuất bản" });
            cboTrangThai.SelectedIndex = 1;
            currentY += 68;

            // 2. Tiêu đề Chapter
            AddLabel("Tiêu đề Chapter", card, 28, currentY);
            txtTieuDe = AddInput(card, 28, currentY + 22, 380);
            currentY += 68;

            // 3. Thư mục PSD
            AddLabel("Đường dẫn Folder PSD / Bản thảo Chapter", card, 28, currentY);
            txtFolderPsd = AddInput(card, 28, currentY + 22, 380);
            currentY += 68;

            // Checkbox tự động tạo pipeline
            chkAutoPipeline = new CheckBox
            {
                Text = "Tự động tạo 4 công đoạn chuẩn: Trans, Clean, Typeset, QC",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.SecondaryPink,
                Location = new Point(28, currentY),
                Size = new Size(380, 24),
                Checked = true,
                Visible = (_existingChapter == null),
                Parent = card
            };
            currentY += 28;

            lblError = new Label
            {
                Text = "",
                Font = StudioColors.SmallFont,
                ForeColor = StudioColors.DangerRed,
                Location = new Point(28, currentY),
                Size = new Size(380, 20),
                Parent = card
            };
            currentY += 24;

            // Buttons
            var btnSave = new RoundedButton
            {
                Text = _existingChapter == null ? "THÊM CHAPTER" : "CẬP NHẬT",
                Location = new Point(28, currentY),
                Size = new Size(185, 38),
                BorderRadius = 8,
                NormalColor = StudioColors.CyanNeon,
                HoverColor = Color.FromArgb(34, 211, 238),
                Parent = card
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new RoundedButton
            {
                Text = "Hủy bỏ",
                Location = new Point(223, currentY),
                Size = new Size(185, 38),
                BorderRadius = 8,
                NormalColor = Color.FromArgb(45, 45, 65),
                HoverColor = Color.FromArgb(60, 60, 85),
                Parent = card
            };
            btnCancel.Click += (s, e) => Close();
        }

        private void AddLabel(string text, Control parent, int x, int y, int width = 380)
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

        private TextBox AddInput(Control parent, int x, int y, int width = 380)
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
            if (_existingChapter == null) return;
            numSoChapter.Value = (decimal)_existingChapter.SoChapter;
            txtTieuDe.Text = _existingChapter.TieuDe ?? "";
            cboTrangThai.SelectedItem = _existingChapter.TrangThai;
            txtFolderPsd.Text = _existingChapter.FolderPsd ?? "";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            double soChap = (double)numSoChapter.Value;

            if (_existingChapter == null)
            {
                var newChap = new Chapter
                {
                    TruyenId = _truyenId,
                    SoChapter = soChap,
                    TieuDe = txtTieuDe.Text.Trim(),
                    TrangThai = cboTrangThai.SelectedItem?.ToString() ?? "Đang biên tập",
                    FolderPsd = txtFolderPsd.Text.Trim()
                };

                var res = _comicService.AddChapter(newChap, chkAutoPipeline.Checked);
                if (!res.Success)
                {
                    lblError.Text = res.Message;
                    return;
                }
            }
            else
            {
                _existingChapter.SoChapter = soChap;
                _existingChapter.TieuDe = txtTieuDe.Text.Trim();
                _existingChapter.TrangThai = cboTrangThai.SelectedItem?.ToString() ?? "Đang biên tập";
                _existingChapter.FolderPsd = txtFolderPsd.Text.Trim();

                var res = _comicService.UpdateChapter(_existingChapter);
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
