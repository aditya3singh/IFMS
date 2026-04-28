using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IFMS.Booking.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations — avoids needing Redis running during migration creation.
/// </summary>
public class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BookingDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=IFMS_BookingDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True");

        return new BookingDbContext(optionsBuilder.Options);
    }
}
