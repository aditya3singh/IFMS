using IFMS.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<FuelStock> FuelStocks => Set<FuelStock>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=IFMS_InventoryDB;User Id=sa;Password=TechUniv2026!;TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FuelStock>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.FuelType).IsRequired().HasMaxLength(50);
            entity.Property(f => f.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(f => f.PricePerLitre).HasColumnType("decimal(18,2)");
            entity.Property(f => f.Status).IsRequired().HasMaxLength(20);
        });
    }
}