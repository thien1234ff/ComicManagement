using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ComicManagement.Services
{
    public static class AppConfiguration
    {
        private static IConfigurationRoot? _configuration;

        public static IConfigurationRoot Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(basePath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                    _configuration = builder.Build();
                }
                return _configuration;
            }
        }

        public static string GetConnectionString(string name = "ComicDbConnection")
        {
            try
            {
                string? conn = Configuration.GetConnectionString(name);
                if (!string.IsNullOrWhiteSpace(conn))
                {
                    return conn;
                }
            }
            catch
            {
                // Fallback nếu không đọc được appsettings.json
            }

            return "Server=.\\SQLEXPRESS;Database=ComicManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
        }

        public static string StudioName => Configuration["StudioSettings:StudioName"] ?? "MANGA CREATIVE EDITORIAL STUDIO HUB";
    }
}
