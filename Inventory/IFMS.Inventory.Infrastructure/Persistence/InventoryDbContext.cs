using IFMS.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<FuelStock> FuelStocks => Set<FuelStock>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<StockDelivery> StockDeliveries => Set<StockDelivery>();

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
        // FuelStock configuration
        modelBuilder.Entity<FuelStock>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.FuelType).IsRequired().HasMaxLength(50);
            entity.Property(f => f.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(f => f.PricePerLitre).HasColumnType("decimal(18,2)");
            entity.Property(f => f.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(f => new { f.StationId, f.FuelType });
        });

        // StockTransaction configuration
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(st => st.Id);
            entity.ToTable("StockTransactions");
            
            entity.Property(st => st.FuelType).IsRequired().HasMaxLength(50);
            entity.Property(st => st.TransactionType).IsRequired().HasMaxLength(20);
            entity.Property(st => st.QuantityChange).HasColumnType("decimal(18,2)");
            entity.Property(st => st.QuantityBefore).HasColumnType("decimal(18,2)");
            entity.Property(st => st.QuantityAfter).HasColumnType("decimal(18,2)");
            entity.Property(st => st.PricePerLitre).HasColumnType("decimal(18,2)");
            entity.Property(st => st.PerformedBy).IsRequired().HasMaxLength(50);
            entity.Property(st => st.Notes).HasMaxLength(500);
            
            entity.HasIndex(st => st.FuelStockId);
            entity.HasIndex(st => st.StationId);
            entity.HasIndex(st => st.CreatedAt);
            entity.HasIndex(st => st.SaleTransactionId);
            
            entity.HasOne(st => st.FuelStock)
                .WithMany()
                .HasForeignKey(st => st.FuelStockId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Supplier configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.ToTable("Suppliers");
            
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.ContactPerson).HasMaxLength(200);
            entity.Property(s => s.Phone).IsRequired().HasMaxLength(20);
            entity.Property(s => s.Email).HasMaxLength(200);
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.Rating).IsRequired().HasMaxLength(20);
            entity.Property(s => s.Status).IsRequired().HasMaxLength(20);
            
            entity.HasIndex(s => s.Name);
            entity.HasIndex(s => s.Status);
        });

        // StockDelivery configuration
        modelBuilder.Entity<StockDelivery>(entity =>
        {
            entity.HasKey(sd => sd.Id);
            entity.ToTable("StockDeliveries");
            
            entity.Property(sd => sd.FuelType).IsRequired().HasMaxLength(50);
            entity.Property(sd => sd.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(sd => sd.PricePerLitre).HasColumnType("decimal(18,2)");
            entity.Property(sd => sd.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(sd => sd.Status).IsRequired().HasMaxLength(20);
            entity.Property(sd => sd.Notes).HasMaxLength(500);
            
            entity.HasIndex(sd => sd.StationId);
            entity.HasIndex(sd => sd.SupplierId);
            entity.HasIndex(sd => sd.Status);
            entity.HasIndex(sd => sd.ScheduledDate);
            
            entity.HasOne(sd => sd.Supplier)
                .WithMany()
                .HasForeignKey(sd => sd.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}