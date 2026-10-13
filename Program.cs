using System;
using System.Windows.Forms;
using ComicManagement.Data;
using ComicManagement.Forms;

namespace ComicManagement
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Khởi tạo và kiểm tra database tự động
            try
            {
                using var db = new ComicDbContext();
                DbInitializer.Initialize(db);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể kết nối đến SQL Server: {ex.Message}\n\nVui lòng kiểm tra lại dịch vụ SQL Server SQLEXPRESS hoặc ConnectionString trong appsettings.json.",
                    "Lỗi Cơ Sở Dữ Liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            Application.Run(new LoginForm());
        }
    }
}