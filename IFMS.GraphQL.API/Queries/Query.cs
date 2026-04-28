using IFMS.Admin.Infrastructure.Persistence;
using IFMS.Booking.Infrastructure.Persistence;
using IFMS.GraphQL.API.Types;
using IFMS.Inventory.Infrastructure.Persistence;
using IFMS.Sales.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IFMS.GraphQL.API.Queries;

public class Query
{
    // ── ADMIN ─────────────────────────────────────────────────────────────────

    /// <summary>System-wide overview: total transactions, revenue, fuel sold by type.</summary>
    public async Task<AdminOverviewType> GetAdminOverview([Service] AdminDbContext db)
    {
        var tx = await db.Transactions.ToListAsync();
        return new AdminOverviewType
        {
            TotalTransactions = tx.Count,
            TotalRevenue      = tx.Sum(t => t.TotalAmount),
            PetrolSold        = tx.Where(t => t.FuelType == "Petrol").Sum(t => t.Quantity),
            DieselSold        = tx.Where(t => t.FuelType == "Diesel").Sum(t => t.Quantity),
            CngSold           = tx.Where(t => t.FuelType == "CNG").Sum(t => t.Quantity)
        };
    }

    /// <summary>Revenue trend grouped by day, week, or month.</summary>
    public async Task<IReadOnlyList<RevenueTrendPoint>> GetRevenueTrend(
        [Service] AdminDbContext db,
        DateTime from,
        DateTime to,
        string groupBy = "day")
    {
        var tx = await db.Transactions
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .ToListAsync();

        IEnumerable<IGrouping<string, IFMS.Admin.Infrastructure.Models.TransactionView>> grouped =
            groupBy.ToLower() switch
            {
                "month" => tx.GroupBy(t => t.TransactionDate.ToString("yyyy-MM")),
                "week"  => tx.GroupBy(t => $"{t.TransactionDate.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(t.TransactionDate):D2}"),
                _       => tx.GroupBy(t => t.TransactionDate.ToString("yyyy-MM-dd"))
            };

        return grouped
            .Select(g => new RevenueTrendPoint
            {
                Period       = g.Key,
                Transactions = g.Count(),
                Revenue      = g.Sum(x => x.TotalAmount),
                Litres       = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.Period)
            .ToList();
    }

    /// <summary>Booking counts by status and total revenue from used bookings.</summary>
    public async Task<BookingsOverviewType> GetBookingsOverview([Service] BookingDbContext db)
    {
        var bookings = await db.Bookings.ToListAsync();
        return new BookingsOverviewType
        {
            Total        = bookings.Count,
            Pending      = bookings.Count(b => b.TokenStatus == "PENDING"),
            Used         = bookings.Count(b => b.TokenStatus == "USED"),
            Cancelled    = bookings.Count(b => b.TokenStatus == "CANCELLED"),
            Expired      = bookings.Count(b => b.TokenStatus == "EXPIRED"),
            TotalRevenue = bookings.Where(b => b.TokenStatus == "USED").Sum(b => b.TotalPaid)
        };
    }

    // ── BOOKINGS ──────────────────────────────────────────────────────────────

    /// <summary>All bookings for a customer, optionally filtered by status.</summary>
    public async Task<IReadOnlyList<BookingType>> GetCustomerBookings(
        [Service] BookingDbContext db,
        Guid customerId,
        string? status = null)
    {
        var q = db.Bookings.Where(b => b.CustomerId == customerId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(b => b.TokenStatus == status.ToUpper());

        var bookings = await q.OrderByDescending(b => b.BookedAt).ToListAsync();
        return bookings.Select(b => new BookingType
        {
            BookingId     = b.BookingId,
            CustomerId    = b.CustomerId,
            StationId     = b.StationId,
            FuelType      = b.FuelType,
            QuantityLiters = b.QuantityLiters,
            TotalPaid     = b.TotalPaid,
            TokenCode     = b.TokenCode,
            TokenStatus   = b.TokenStatus,
            PaymentId     = b.PaymentId,
            BookedAt      = b.BookedAt,
            ExpiresAt     = b.ExpiresAt,
            UsedAt        = b.UsedAt
        }).ToList();
    }

    /// <summary>All bookings for a station, optionally filtered by date range.</summary>
    public async Task<IReadOnlyList<BookingType>> GetStationBookings(
        [Service] BookingDbContext db,
        Guid stationId,
        DateTime? from = null,
        DateTime? to = null)
    {
        var q = db.Bookings.Where(b => b.StationId == stationId);
        if (from.HasValue) q = q.Where(b => b.BookedAt >= from.Value);
        if (to.HasValue)   q = q.Where(b => b.BookedAt <= to.Value);

        var bookings = await q.OrderByDescending(b => b.BookedAt).ToListAsync();
        return bookings.Select(b => new BookingType
        {
            BookingId      = b.BookingId,
            CustomerId     = b.CustomerId,
            StationId      = b.StationId,
            FuelType       = b.FuelType,
            QuantityLiters = b.QuantityLiters,
            TotalPaid      = b.TotalPaid,
            TokenCode      = b.TokenCode,
            TokenStatus    = b.TokenStatus,
            PaymentId      = b.PaymentId,
            BookedAt       = b.BookedAt,
            ExpiresAt      = b.ExpiresAt,
            UsedAt         = b.UsedAt
        }).ToList();
    }

    // ── TRANSACTIONS ──────────────────────────────────────────────────────────

    /// <summary>Transactions for one or more stations, optionally filtered by date.</summary>
    public async Task<IReadOnlyList<TransactionType>> GetTransactions(
        [Service] SalesDbContext db,
        Guid? stationId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var q = db.Transactions.AsQueryable();
        if (stationId.HasValue) q = q.Where(t => t.StationId == stationId.Value);
        if (from.HasValue)      q = q.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue)        q = q.Where(t => t.TransactionDate <= to.Value);

        var tx = await q.OrderByDescending(t => t.TransactionDate).ToListAsync();
        return tx.Select(t => new TransactionType
        {
            Id              = t.Id,
            StationId       = t.StationId,
            FuelType        = t.FuelType,
            Quantity        = t.Quantity,
            PricePerLitre   = t.PricePerLitre,
            TotalAmount     = t.TotalAmount,
            PaymentMethod   = t.PaymentMethod,
            Status          = t.Status,
            TransactionDate = t.TransactionDate,
            CustomerName    = t.CustomerName
        }).ToList();
    }

    // ── INVENTORY ─────────────────────────────────────────────────────────────

    /// <summary>Fuel stocks, optionally filtered by station or low-stock flag.</summary>
    public async Task<IReadOnlyList<FuelStockType>> GetFuelStocks(
        [Service] InventoryDbContext db,
        Guid? stationId = null,
        bool lowStockOnly = false)
    {
        var q = db.FuelStocks.AsQueryable();
        if (stationId.HasValue) q = q.Where(f => f.StationId == stationId.Value);
        if (lowStockOnly)       q = q.Where(f => f.Quantity < 500);

        var stocks = await q.ToListAsync();
        return stocks.Select(f => new FuelStockType
        {
            Id           = f.Id,
            FuelType     = f.FuelType,
            Quantity     = f.Quantity,
            PricePerLitre = f.PricePerLitre,
            Status       = f.Status,
            StationId    = f.StationId,
            LastUpdated  = f.LastUpdated,
            IsLowStock   = f.Quantity < 500
        }).ToList();
    }

    // ── STATIONS ──────────────────────────────────────────────────────────────

    /// <summary>All active stations, optionally filtered by city or state.</summary>
    public async Task<IReadOnlyList<StationType>> GetStations(
        [Service] StationDbContext db,
        string? city = null,
        string? state = null)
    {
        var q = db.Stations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(city))  q = q.Where(s => s.City == city);
        if (!string.IsNullOrWhiteSpace(state)) q = q.Where(s => s.State == state);

        var stations = await q.OrderBy(s => s.Name).ToListAsync();
        return stations.Select(s => new StationType
        {
            Id            = s.Id,
            Name          = s.Name,
            LicenseNumber = s.LicenseNumber,
            City          = s.City,
            State         = s.State,
            Latitude      = s.Latitude,
            Longitude     = s.Longitude,
            IsActive      = s.IsActive,
            CreatedAt     = s.CreatedAt,
            UpdatedAt     = s.UpdatedAt
        }).ToList();
    }
}
