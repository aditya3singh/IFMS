using IFMS.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Sales.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Complaint> Complaints => Set<Complaint>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=IFMS_SalesDB;User Id=sa;Password=TechUniv2026!;TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.FuelType).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(t => t.PricePerLitre).HasColumnType("decimal(18,2)");
            entity.Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Status).IsRequired().HasMaxLength(20);
            entity.Property(t => t.CustomerName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CustomerId).IsRequired();
            entity.Property(c => c.CustomerName).HasMaxLength(100);
            entity.Property(c => c.CustomerEmail).HasMaxLength(150);
            entity.Property(c => c.CustomerPhone).HasMaxLength(20);
            entity.Property(c => c.Category).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Subject).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).IsRequired().HasMaxLength(2000);
            entity.Property(c => c.ReferenceId).HasMaxLength(50);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
            entity.Property(c => c.ResolutionNote).HasMaxLength(1000);
            entity.HasIndex(c => c.CustomerId);
            entity.HasIndex(c => c.Status);
        });
    }
}