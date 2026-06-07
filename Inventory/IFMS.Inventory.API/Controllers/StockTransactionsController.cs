using IFMS.Inventory.Application.Interfaces;
using IFMS.Inventory.Application.DTOs;
using IFMS.Station.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Dealer")]
public class StockTransactionsController : ControllerBase
{
    private readonly IStockTransactionRepository _repository;
    private readonly IDealerAssignmentRepository _dealerAssignments;
    private readonly ILogger<StockTransactionsController> _logger;

    public StockTransactionsController(
        IStockTransactionRepository repository,
        IDealerAssignmentRepository dealerAssignments,
        ILogger<StockTransactionsController> logger)
    {
        _repository = repository;
        _dealerAssignments = dealerAssignments;
        _logger = logger;
    }

    [HttpGet("station/{stationId:guid}")]
    public async Task<IActionResult> GetByStation(Guid stationId, [FromQuery] int limit = 100)
    {
        try
        {
            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, stationId))
                    return Forbid();
            }

            var transactions = await _repository.GetByStationIdAsync(stationId, limit);
            var result = transactions.Select(MapToResponse).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for station {StationId}", stationId);
            return StatusCode(500, new { error = "Failed to retrieve transactions" });
        }
    }

    [HttpGet("fuel-stock/{fuelStockId:guid}")]
    public async Task<IActionResult> GetByFuelStock(Guid fuelStockId)
    {
        try
        {
            var transactions = await _repository.GetByFuelStockIdAsync(fuelStockId);
            
            // Check dealer access
            if (User.IsInRole("Dealer") && transactions.Any())
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                var stationId = transactions.First().StationId;
                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, stationId))
                    return Forbid();
            }

            var result = transactions.Select(MapToResponse).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for fuel stock {FuelStockId}", fuelStockId);
            return StatusCode(500, new { error = "Failed to retrieve transactions" });
        }
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int days = 30, [FromQuery] int limit = 1000)
    {
        try
        {
            var transactions = await _repository.GetRecentAsync(days, limit);

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
                transactions = transactions.Where(t => stationIds.Contains(t.StationId)).ToList();
            }

            var result = transactions.Select(MapToResponse).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent transactions");
            return StatusCode(500, new { error = "Failed to retrieve transactions" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
                return NotFound(new { error = "Transaction not found" });

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, transaction.StationId))
                    return Forbid();
            }

            return Ok(MapToResponse(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, new { error = "Failed to retrieve transaction" });
        }
    }

    private Guid? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var g) ? g : null;
    }

    private static StockTransactionResponse MapToResponse(Domain.Entities.StockTransaction t) => new(
        t.Id,
        t.FuelStockId,
        t.StationId,
        t.FuelType,
        t.TransactionType,
        t.QuantityChange,
        t.QuantityBefore,
        t.QuantityAfter,
        t.PricePerLitre,
        t.UserId,
        t.PerformedBy,
        t.Notes,
        t.SaleTransactionId,
        t.DeliveryId,
        t.CreatedAt
    );
}
