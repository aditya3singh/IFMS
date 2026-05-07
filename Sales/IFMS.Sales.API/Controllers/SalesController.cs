using IFMS.Sales.Application.Commands;
using IFMS.Sales.Application.DTOs;
using IFMS.Sales.API.Services;
using IFMS.Station.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Dealer")]
public class SalesController : ControllerBase
{
    private readonly TransactionCommandHandler _handler;
    private readonly IDealerAssignmentRepository _dealerAssignments;
    private readonly ISalesNotificationPublisher _notifications;

    public SalesController(
        TransactionCommandHandler handler,
        IDealerAssignmentRepository dealerAssignments,
        ISalesNotificationPublisher notifications)
    {
        _handler = handler;
        _dealerAssignments = dealerAssignments;
        _notifications = notifications;
    }

    /// <summary>
    /// Get all sales transactions with pagination (Dealer: assigned stations only)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (pageSize > 100) pageSize = 100; // Max 100 per page
        if (page < 1) page = 1;

        var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        var allResults = await _handler.GetByStationIdsAsync(stationIds);
        
        var totalCount = allResults.Count;
        var paginatedResults = allResults
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            data = paginatedResults,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>
    /// Get sales transactions for a specific station with pagination
    /// </summary>
    [HttpGet("station/{stationId:guid}")]
    public async Task<IActionResult> GetByStation(Guid stationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!await CanAccessStationAsync(stationId))
            return Forbid();

        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var allResults = await _handler.GetByStationAsync(stationId);
        
        var totalCount = allResults.Count;
        var paginatedResults = allResults
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            data = paginatedResults,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("revenue/{stationId:guid}")]
    public async Task<IActionResult> GetRevenue(Guid stationId)
    {
        if (!await CanAccessStationAsync(stationId))
            return Forbid();

        var result = await _handler.GetTotalRevenueAsync(stationId);
        return Ok(new { stationId, totalRevenue = result });
    }

    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        var result = await _handler.GetByDateRangeForStationsAsync(from, to, stationIds);
        return Ok(result);
    }

    /// <summary>
    /// Create a sales transaction (Dealer: for walk-in customers, Admin: for corrections)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Dealer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        // Dealer can only create for their assigned stations
        if (User.IsInRole("Dealer"))
        {
            if (!await CanAccessStationAsync(request.StationId))
                return Forbid();
        }

        try
        {
            var result = await _handler.CreateAsync(request);

            // Fire-and-forget: push in-app notification to dealer + admin
            _ = _notifications.PushSaleRecordedAsync(
                result.StationId,
                result.FuelType,
                result.Quantity,
                result.TotalAmount,
                result.CustomerName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Internal service-to-service endpoint: create a transaction from a confirmed booking.
    /// Called by the Booking API — no user JWT available in that context.
    /// </summary>
    [HttpPost("internal/from-booking")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateFromBooking([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var result = await _handler.CreateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get a single transaction by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _handler.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Transaction not found." });

        if (User.IsInRole("Dealer") && !await CanAccessStationAsync(result.StationId))
            return Forbid();

        return Ok(result);
    }

    /// <summary>Aggregated summary: total revenue, litres, breakdown by fuel type and payment method.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        IReadOnlyCollection<Guid> stationIds;
        if (User.IsInRole("Admin"))
        {
            // Admin sees all — pass empty list to get all
            var all = await _handler.GetAllAsync();
            stationIds = all.Select(t => t.StationId).Distinct().ToList();
        }
        else
        {
            stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        }

        var result = await _handler.GetSalesSummaryAsync(stationIds);
        return Ok(result);
    }

    /// <summary>Revenue trend grouped by day/week/month for charts.</summary>
    [HttpGet("revenue-trend")]
    public async Task<IActionResult> GetRevenueTrend(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string groupBy = "day")
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        IReadOnlyCollection<Guid> stationIds;
        if (User.IsInRole("Admin"))
        {
            var all = await _handler.GetAllAsync();
            stationIds = all.Select(t => t.StationId).Distinct().ToList();
        }
        else
        {
            stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        }

        var result = await _handler.GetRevenueTrendAsync(stationIds, from, to, groupBy);
        return Ok(result);
    }

    /// <summary>Export transactions as JSON for a date range.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        IReadOnlyCollection<Guid> stationIds;
        if (User.IsInRole("Admin"))
        {
            var all = await _handler.GetAllAsync();
            stationIds = all.Select(t => t.StationId).Distinct().ToList();
        }
        else
        {
            stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        }

        var result = await _handler.GetExportAsync(stationIds, from, to);
        return Ok(new { exportedAt = DateTime.UtcNow, from, to, count = result.Count, data = result });
    }

    /// <summary>Breakdown of sales by fuel type.</summary>
    [HttpGet("by-fuel-type")]
    public async Task<IActionResult> GetByFuelType()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        var transactions = await _handler.GetByStationIdsAsync(stationIds);

        var breakdown = transactions
            .GroupBy(t => t.FuelType)
            .Select(g => new
            {
                fuelType = g.Key,
                count = g.Count(),
                totalLitres = g.Sum(x => x.Quantity),
                totalRevenue = g.Sum(x => x.TotalAmount),
                avgPricePerLitre = g.Average(x => x.PricePerLitre)
            })
            .OrderByDescending(x => x.totalRevenue)
            .ToList();

        return Ok(breakdown);
    }

    private Guid? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var g) ? g : null;
    }

    private async Task<bool> CanAccessStationAsync(Guid stationId)
    {
        if (User.IsInRole("Admin"))
            return true;

        if (User.IsInRole("Dealer"))
        {
            var userId = GetUserId();
            if (userId == null)
                return false;

            return await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, stationId);
        }

        return false;
    }
}
