using IFMS.Admin.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<TransactionView> Transactions => Set<TransactionView>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=IFMS_SalesDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionView>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);
        });
    }
}