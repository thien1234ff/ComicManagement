using System;
using Microsoft.EntityFrameworkCore;
using ComicManagement.Models;
using ComicManagement.Services;

namespace ComicManagement.Data
{
    public class ComicDbContext : DbContext
    {
        public DbSet<ThanhVien> ThanhViens { get; set; } = null!;
        public DbSet<Truyen> Truyens { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;
        public DbSet<CongDoanDeadline> CongDoanDeadlines { get; set; } = null!;
        public DbSet<ThongBao> ThongBaos { get; set; } = null!;

        public ComicDbContext()
        {
        }

        public ComicDbContext(DbContextOptions<ComicDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connStr = AppConfiguration.GetConnectionString();
                optionsBuilder.UseSqlServer(connStr);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ThanhVien
            modelBuilder.Entity<ThanhVien>(entity =>
            {
                entity.HasIndex(e => e.TenDangNhap).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Truyen -> Chapter (Cascade Delete)
            modelBuilder.Entity<Chapter>()
                .HasOne(c => c.Truyen)
                .WithMany(t => t.Chapters)
                .HasForeignKey(c => c.TruyenId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Chapter -> CongDoanDeadline (Cascade Delete)
            modelBuilder.Entity<CongDoanDeadline>()
                .HasOne(cd => cd.Chapter)
                .WithMany(c => c.CacCongDoan)
                .HasForeignKey(cd => cd.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ThanhVien -> CongDoanDeadline (Set Null on Delete)
            modelBuilder.Entity<CongDoanDeadline>()
                .HasOne(cd => cd.ThanhVien)
                .WithMany(tv => tv.CacCongDoan)
                .HasForeignKey(cd => cd.ThanhVienId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure ThanhVien -> ThongBao (Cascade Delete)
            modelBuilder.Entity<ThongBao>()
                .HasOne(tb => tb.NguoiNhan)
                .WithMany(tv => tv.CacThongBao)
                .HasForeignKey(tb => tb.NguoiNhanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
