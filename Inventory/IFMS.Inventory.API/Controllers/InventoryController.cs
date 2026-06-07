using IFMS.Inventory.Application.Commands;
using IFMS.Inventory.Application.DTOs;
using IFMS.Inventory.API.Services;
using IFMS.Messaging.Events;
using IFMS.Station.Application.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly FuelStockCommandHandler _handler;
    private readonly IDealerAssignmentRepository _dealerAssignments;
    private readonly IPublishEndpoint _publish;

    public InventoryController(
        FuelStockCommandHandler handler,
        IDealerAssignmentRepository dealerAssignments,
        IPublishEndpoint publish)
    {
        _handler = handler;
        _dealerAssignments = dealerAssignments;
        _publish = publish;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (User.IsInRole("Admin"))
        {
            var result = await _handler.GetAllAsync();
            return Ok(result);
        }

        if (User.IsInRole("Dealer"))
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
            var result = await _handler.GetByStationIdsAsync(stationIds);
            return Ok(result);
        }

        return Forbid();
    }

    [HttpGet("station/{stationId:guid}")]
    [AllowAnonymous] // Customers need to see inventory to book fuel
    public async Task<IActionResult> GetByStation(Guid stationId)
    {
        // Allow all authenticated users (Admin, Dealer, Customer) to view station inventory
        // This is read-only and needed for booking flow
        var result = await _handler.GetByStationAsync(stationId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> Create([FromBody] CreateFuelStockRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (User.IsInRole("Dealer"))
        {
            if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, request.StationId))
                return Forbid();
        }

        try
        {
            var result = await _handler.CreateAsync(request, userId, User.IsInRole("Admin") ? "Admin" : "Dealer");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("update-stock")]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> UpdateStock([FromBody] UpdateStockRequest request)
    {
        var existing = await _handler.GetByIdAsync(request.Id);
        if (existing == null)
            return NotFound(new { error = "Fuel stock not found." });

        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (User.IsInRole("Dealer"))
        {
            if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, existing.StationId))
                return Forbid();
        }

        try
        {
            var result = await _handler.UpdateStockAsync(
                request,
                userId,
                User.IsInRole("Admin") ? "Admin" : "Dealer",
                "Manual stock adjustment"
            );

            // Publish LowStockAlert event → Notification API consumes via RabbitMQ
            if (result.IsLowStock)
                _ = _publish.Publish(new LowStockAlert(result.StationId, result.FuelType, result.Quantity, 500));

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Internal endpoint for service-to-service inventory deduction (no auth required)
    /// </summary>
    [HttpPut("internal/deduct")]
    [AllowAnonymous]
    public async Task<IActionResult> DeductStock([FromBody] UpdateStockRequest request)
    {
        var existing = await _handler.GetByIdAsync(request.Id);
        if (existing == null)
            return NotFound(new { error = "Fuel stock not found." });

        try
        {
            var result = await _handler.UpdateStockAsync(
                request,
                null,
                "Sale",
                "Automatic deduction from sale transaction"
            );

            // Publish LowStockAlert event → Notification API consumes via RabbitMQ
            if (result.IsLowStock)
                _ = _publish.Publish(new LowStockAlert(result.StationId, result.FuelType, result.Quantity, 500));

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>Get a single fuel stock record by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _handler.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Fuel stock not found." });

        if (User.IsInRole("Dealer"))
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!await _dealerAssignments.UserIsAssignedToStationAsync(userId.Value, result.StationId))
                return Forbid();
        }

        return Ok(result);
    }

    /// <summary>Delete a fuel stock record. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _handler.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>Get all stations with stock below threshold (default 500L). Admin and Dealer.</summary>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetLowStock([FromQuery] decimal threshold = 500)
    {
        if (User.IsInRole("Admin"))
        {
            var all = await _handler.GetLowStockAsync(threshold);
            return Ok(new { count = all.Count, threshold, items = all });
        }

        // Dealer: only their stations
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var stationIds = await _dealerAssignments.GetStationIdsForUserAsync(userId.Value);
        var result = (await _handler.GetLowStockAsync(threshold))
            .Where(s => stationIds.Contains(s.StationId))
            .ToList();
        return Ok(new { count = result.Count, threshold, items = result });
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
