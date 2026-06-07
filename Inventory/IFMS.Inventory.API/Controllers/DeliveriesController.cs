using IFMS.Inventory.Application.Commands;
using IFMS.Inventory.Application.DTOs;
using IFMS.Station.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly StockDeliveryCommandHandler _handler;
    private readonly IDealerAssignmentRepository _dealerAssignments;
    private readonly ILogger<DeliveriesController> _logger;

    public DeliveriesController(
        StockDeliveryCommandHandler handler,
        IDealerAssignmentRepository dealerAssignments,
        ILogger<DeliveriesController> logger)
    {
        _handler = handler;
        _dealerAssignments = dealerAssignments;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            if (User.IsInRole("Admin"))
            {
                var result = await _handler.GetAllAsync();
                return Ok(result);
            }

            // Dealer: only their stations' deliveries
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
            var allDeliveries = await _handler.GetAllAsync();
            var filteredDeliveries = allDeliveries
                .Where(d => stationIds.Contains(d.StationId))
                .ToList();

            return Ok(filteredDeliveries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deliveries");
            return StatusCode(500, new { error = "Failed to retrieve deliveries" });
        }
    }

    [HttpGet("station/{stationId:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetByStation(Guid stationId)
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

            var result = await _handler.GetByStationIdAsync(stationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deliveries for station {StationId}", stationId);
            return StatusCode(500, new { error = "Failed to retrieve deliveries" });
        }
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        try
        {
            var result = await _handler.GetByStatusAsync(status);

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
                result = result.Where(d => stationIds.Contains(d.StationId)).ToList();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deliveries by status {Status}", status);
            return StatusCode(500, new { error = "Failed to retrieve deliveries" });
        }
    }

    [HttpGet("upcoming")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7)
    {
        try
        {
            var result = await _handler.GetUpcomingAsync(days);

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
                result = result.Where(d => stationIds.Contains(d.StationId)).ToList();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming deliveries");
            return StatusCode(500, new { error = "Failed to retrieve deliveries" });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _handler.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { error = "Delivery not found" });

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, result.StationId))
                    return Forbid();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delivery {DeliveryId}", id);
            return StatusCode(500, new { error = "Failed to retrieve delivery" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> Create([FromBody] CreateStockDeliveryRequest request)
    {
        try
        {
            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, request.StationId))
                    return Forbid();

                var result = await _handler.CreateAsync(request, userId.Value);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            else
            {
                // Admin
                var adminUserId = GetUserId() ?? Guid.Empty;
                var result = await _handler.CreateAsync(request, adminUserId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delivery");
            return StatusCode(500, new { error = "Failed to create delivery" });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockDeliveryRequest request)
    {
        try
        {
            var existing = await _handler.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Delivery not found" });

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, existing.StationId))
                    return Forbid();
            }

            var result = await _handler.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery {DeliveryId}", id);
            return StatusCode(500, new { error = "Failed to update delivery" });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDeliveryStatusRequest request)
    {
        try
        {
            var existing = await _handler.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Delivery not found" });

            var userId = GetUserId();

            if (User.IsInRole("Dealer"))
            {
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, existing.StationId))
                    return Forbid();
            }

            var result = await _handler.UpdateStatusAsync(id, request, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery status {DeliveryId}", id);
            return StatusCode(500, new { error = "Failed to update delivery status" });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var existing = await _handler.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Delivery not found" });

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized();

                if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, existing.StationId))
                    return Forbid();
            }

            await _handler.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting delivery {DeliveryId}", id);
            return StatusCode(500, new { error = "Failed to delete delivery" });
        }
    }

    private Guid? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var g) ? g : null;
    }
}
