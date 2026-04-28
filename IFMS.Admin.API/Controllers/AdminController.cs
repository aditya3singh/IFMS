using IFMS.Admin.Application.DTOs;
using IFMS.Admin.Infrastructure.Persistence;
using IFMS.Booking.Infrastructure.Persistence;
using IFMS.Inventory.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminDbContext _context;
    private readonly StationDbContext _stationDb;
    private readonly BookingDbContext _bookingDb;
    private readonly InventoryDbContext _inventoryDb;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        AdminDbContext context,
        StationDbContext stationDb,
        BookingDbContext bookingDb,
        InventoryDbContext inventoryDb,
        IConfiguration configuration,
        ILogger<AdminController> logger)
    {
        _context = context;
        _stationDb = stationDb;
        _bookingDb = bookingDb;
        _inventoryDb = inventoryDb;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var transactions = await _context.Set<IFMS.Admin.Infrastructure.Models.TransactionView>()
            .ToListAsync();

        return Ok(new
        {
            totalTransactions = transactions.Count,
            totalRevenue = transactions.Sum(t => t.TotalAmount),
            petrolSold = transactions.Where(t => t.FuelType == "Petrol").Sum(t => t.Quantity),
            dieselSold = transactions.Where(t => t.FuelType == "Diesel").Sum(t => t.Quantity),
            cngSold = transactions.Where(t => t.FuelType == "CNG").Sum(t => t.Quantity)
        });
    }

    [HttpGet("daily-report")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
    {
        var startDate = date.Date;
        var endDate = date.Date.AddDays(1);

        var transactions = await _context.Set<IFMS.Admin.Infrastructure.Models.TransactionView>()
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate)
            .ToListAsync();

        return Ok(new DailyReportResponse(
            date.Date,
            transactions.Count,
            transactions.Sum(t => t.TotalAmount),
            transactions.Where(t => t.FuelType == "Petrol").Sum(t => t.Quantity),
            transactions.Where(t => t.FuelType == "Diesel").Sum(t => t.Quantity),
            transactions.Where(t => t.FuelType == "CNG").Sum(t => t.Quantity)
        ));
    }

    /// <summary>
    /// Fraud detection endpoint — flags transactions where:
    /// 1. Single transaction TotalAmount > ₹50,000 (threshold)
    /// 2. Quantity > 500L in a single transaction
    /// 3. Multiple transactions at same station within 1 minute
    /// </summary>
    [HttpGet("fraud-monitor")]
    public async Task<IActionResult> GetFraudAlerts()
    {
        var transactions = await _context.Set<IFMS.Admin.Infrastructure.Models.TransactionView>()
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        var flagged = new List<object>();

        // Rule 1: High-value transactions (> ₹50,000)
        var highValue = transactions
            .Where(t => t.TotalAmount > 50000)
            .Select(t => new
            {
                t.Id,
                t.StationId,
                t.FuelType,
                t.Quantity,
                t.TotalAmount,
                t.TransactionDate,
                Reason = $"High value transaction: ₹{t.TotalAmount:N2}"
            });
        flagged.AddRange(highValue);

        // Rule 2: Unusually large quantity (> 500L)
        var largeQty = transactions
            .Where(t => t.Quantity > 500 && t.TotalAmount <= 50000)
            .Select(t => new
            {
                t.Id,
                t.StationId,
                t.FuelType,
                t.Quantity,
                t.TotalAmount,
                t.TransactionDate,
                Reason = $"Unusually large quantity: {t.Quantity:N2}L"
            });
        flagged.AddRange(largeQty);

        // Rule 3: Rapid successive transactions at same station (within 1 minute)
        var grouped = transactions
            .GroupBy(t => t.StationId)
            .SelectMany(g =>
            {
                var ordered = g.OrderBy(t => t.TransactionDate).ToList();
                var rapid = new List<object>();
                for (int i = 1; i < ordered.Count; i++)
                {
                    if ((ordered[i].TransactionDate - ordered[i - 1].TransactionDate).TotalMinutes < 1)
                    {
                        rapid.Add(new
                        {
                            ordered[i].Id,
                            ordered[i].StationId,
                            ordered[i].FuelType,
                            ordered[i].Quantity,
                            ordered[i].TotalAmount,
                            ordered[i].TransactionDate,
                            Reason = "Rapid successive transaction (< 1 min gap)"
                        });
                    }
                }
                return rapid;
            });
        flagged.AddRange(grouped);

        return Ok(new
        {
            totalFlagged = flagged.Count,
            alerts = flagged
        });
    }

    /// <summary>
    /// Centralized station monitoring (read-only): status/condition, assigned dealer, visit counts, transactions, revenue, and fraud flags.
    /// Customer-level records are intentionally omitted.
    /// </summary>
    [HttpGet("station-monitor")]
    public async Task<IActionResult> GetStationMonitor()
    {
        try
        {
            var stations = await _stationDb.Stations
                .Include(s => s.DealerAssignment)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var bookingsByStation = await _bookingDb.Bookings
                .GroupBy(b => b.StationId)
                .Select(g => new { StationId = g.Key, VisitCount = g.Count() })
                .ToListAsync();
            var bookingMap = bookingsByStation.ToDictionary(x => x.StationId, x => x.VisitCount);

            var tx = await _context.Transactions
                .AsNoTracking()
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var txAggMap = tx
                .GroupBy(t => t.StationId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Count = g.Count(),
                        Revenue = g.Sum(x => x.TotalAmount),
                        LastAt = g.Max(x => x.TransactionDate)
                    });

            var fraudCountByStation = tx
                .GroupBy(t => t.StationId)
                .ToDictionary(g => g.Key, g => CountFraudAlerts(g.OrderBy(x => x.TransactionDate).ToList()));

            var now = DateTime.UtcNow;
            var rows = stations.Select(s =>
            {
                txAggMap.TryGetValue(s.Id, out var txAgg);
                bookingMap.TryGetValue(s.Id, out var visits);
                fraudCountByStation.TryGetValue(s.Id, out var fraudCount);

                var lastAt = txAgg?.LastAt;
                var condition =
                    !s.IsActive ? "Inactive" :
                    fraudCount > 0 ? "Alert" :
                    (lastAt.HasValue && (now - lastAt.Value).TotalHours > 24) ? "Idle" :
                    "Normal";

                return new
                {
                    stationId = s.Id,
                    stationName = s.Name,
                    city = s.City,
                    state = s.State,
                    isActive = s.IsActive,
                    condition,
                    assignedDealerUserId = s.DealerAssignment?.UserId,
                    customerVisitCount = visits,
                    transactionCount = txAgg?.Count ?? 0,
                    totalRevenue = txAgg?.Revenue ?? 0m,
                    lastTransactionAt = lastAt,
                    fraudAlertCount = fraudCount
                };
            });

            return Ok(new
            {
                totalStations = rows.Count(),
                rows
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading station monitor data");
            return StatusCode(500, new { error = $"Failed to load station monitoring data: {ex.Message}" });
        }
    }

    /// <summary>
    /// Inventory monitoring endpoint for Admin - read-only view of all fuel stocks across all stations
    /// </summary>
    [HttpGet("inventory-monitor")]
    public async Task<IActionResult> GetInventoryMonitor()
    {
        try
        {
            var fuelStocks = await _inventoryDb.FuelStocks
                .OrderByDescending(f => f.LastUpdated)
                .ToListAsync();

            var stocks = fuelStocks.Select(f => new
            {
                id = f.Id,
                stationId = f.StationId,
                fuelType = f.FuelType,
                currentStock = f.Quantity,
                pricePerLitre = f.PricePerLitre,
                status = f.Status,
                lastUpdated = f.LastUpdated,
                isLow = f.Quantity < 1000 // Flag if stock is below 1000L
            });

            return Ok(new
            {
                totalStockRecords = fuelStocks.Count,
                stocks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory monitor data");
            return StatusCode(500, new { error = $"Failed to load inventory data: {ex.Message}" });
        }
    }

    /// <summary>Revenue grouped by day/week/month for charts.</summary>
    [HttpGet("revenue-trend")]
    public async Task<IActionResult> GetRevenueTrend(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string groupBy = "day")
    {
        var transactions = await _context.Set<IFMS.Admin.Infrastructure.Models.TransactionView>()
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .ToListAsync();

        IEnumerable<IGrouping<string, IFMS.Admin.Infrastructure.Models.TransactionView>> grouped = groupBy.ToLower() switch
        {
            "month" => transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM")),
            "week"  => transactions.GroupBy(t => $"{t.TransactionDate.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(t.TransactionDate):D2}"),
            _       => transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM-dd"))
        };

        var result = grouped
            .Select(g => new
            {
                period = g.Key,
                transactions = g.Count(),
                revenue = g.Sum(x => x.TotalAmount),
                litres = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.period)
            .ToList();

        return Ok(new { from, to, groupBy, data = result });
    }

    /// <summary>Top N stations by revenue.</summary>
    [HttpGet("top-stations")]
    public async Task<IActionResult> GetTopStations([FromQuery] int top = 5)
    {
        var transactions = await _context.Set<IFMS.Admin.Infrastructure.Models.TransactionView>()
            .ToListAsync();

        var result = transactions
            .GroupBy(t => t.StationId)
            .Select(g => new
            {
                stationId = g.Key,
                totalRevenue = g.Sum(x => x.TotalAmount),
                totalLitres = g.Sum(x => x.Quantity),
                transactionCount = g.Count()
            })
            .OrderByDescending(x => x.totalRevenue)
            .Take(top)
            .ToList();

        return Ok(new { top, stations = result });
    }

    /// <summary>Booking stats overview: counts by status.</summary>
    [HttpGet("bookings-overview")]
    public async Task<IActionResult> GetBookingsOverview()
    {
        try
        {
            var bookings = await _bookingDb.Bookings.ToListAsync();
            return Ok(new
            {
                total = bookings.Count,
                pending = bookings.Count(b => b.TokenStatus == "PENDING"),
                used = bookings.Count(b => b.TokenStatus == "USED"),
                cancelled = bookings.Count(b => b.TokenStatus == "CANCELLED"),
                expired = bookings.Count(b => b.TokenStatus == "EXPIRED"),
                totalRevenue = bookings.Where(b => b.TokenStatus == "USED").Sum(b => b.TotalPaid)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bookings overview");
            return StatusCode(500, new { error = $"Failed to load bookings overview: {ex.Message}" });
        }
    }

    /// <summary>Dismiss a fraud alert by transaction ID (marks it as reviewed).</summary>
    [HttpPost("fraud-monitor/{transactionId:guid}/dismiss")]
    public IActionResult DismissFraudAlert(Guid transactionId)
    {
        // In a production system this would persist a "dismissed" flag.
        // For now we return 200 to acknowledge the action — the frontend
        // can filter dismissed IDs client-side.
        _logger.LogInformation("Fraud alert dismissed for transaction {TransactionId} by admin", transactionId);
        return Ok(new { message = "Alert dismissed.", transactionId });
    }

    private static int CountFraudAlerts(IReadOnlyList<IFMS.Admin.Infrastructure.Models.TransactionView> orderedTx)
    {
        var count = 0;
        for (int i = 0; i < orderedTx.Count; i++)
        {
            var t = orderedTx[i];
            if (t.TotalAmount > 50000m || t.Quantity > 500m)
                count++;

            if (i > 0 && (orderedTx[i].TransactionDate - orderedTx[i - 1].TransactionDate).TotalMinutes < 1)
                count++;
        }
        return count;
    }
}