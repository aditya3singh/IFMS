using Microsoft.EntityFrameworkCore;

namespace IFMS.Booking.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.Booking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Booking>(entity =>
        {
            entity.HasKey(b => b.BookingId);
            entity.Property(b => b.BookingId).HasDefaultValueSql("NEWID()");
            entity.Property(b => b.FuelType).IsRequired().HasMaxLength(20);
            entity.Property(b => b.QuantityLiters).HasColumnType("decimal(10,2)");
            entity.Property(b => b.TotalPaid).HasColumnType("decimal(12,2)");
            entity.Property(b => b.TokenCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(b => b.TokenCode).IsUnique();
            entity.Property(b => b.TokenStatus).IsRequired().HasMaxLength(20).HasDefaultValue("PENDING");
            entity.Property(b => b.PaymentId).IsRequired().HasMaxLength(100);
            entity.Property(b => b.BookedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(b => b.CustomerPhone).HasMaxLength(20).HasDefaultValue(string.Empty);
            entity.Property(b => b.CustomerEmail).HasMaxLength(150).HasDefaultValue(string.Empty);
        });
    }
}
