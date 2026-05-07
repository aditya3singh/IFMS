using IFMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=IFMS_IdentityDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.GoogleSubjectId).HasMaxLength(255);
            entity.HasIndex(u => u.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
            entity.HasIndex(u => u.GoogleSubjectId).IsUnique().HasFilter("[GoogleSubjectId] IS NOT NULL");
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<OtpChallenge>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.NormalizedKey).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Purpose).IsRequired().HasMaxLength(32);
            entity.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);
            entity.HasIndex(o => new { o.NormalizedKey, o.Purpose });
        });
    }
}