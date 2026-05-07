using IFMS.Sales.Application.Commands;
using IFMS.Sales.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Sales.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly ComplaintCommandHandler _handler;
    private readonly ILogger<ComplaintsController> _logger;

    public ComplaintsController(ComplaintCommandHandler handler, ILogger<ComplaintsController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    /// <summary>Customer: raise a new complaint.</summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Raise([FromBody] RaiseComplaintRequest request)
    {
        var customerId = GetUserId();
        if (customerId == null) return Unauthorized(new { error = "Invalid identity" });

        // Enrich with JWT claims if not provided in body
        var name  = User.FindFirstValue(ClaimTypes.Name) ?? request.CustomerName;
        var email = User.FindFirstValue(ClaimTypes.Email) ?? request.CustomerEmail;
        var phone = User.FindFirstValue(ClaimTypes.MobilePhone) ?? request.CustomerPhone;

        var enriched = request with
        {
            CustomerName  = name,
            CustomerEmail = email,
            CustomerPhone = phone
        };

        try
        {
            var result = await _handler.RaiseAsync(customerId.Value, enriched);
            _logger.LogInformation("Complaint raised by customer {CustomerId}: {Subject}", customerId, request.Subject);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Customer: view own complaints.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine()
    {
        var customerId = GetUserId();
        if (customerId == null) return Unauthorized(new { error = "Invalid identity" });

        var result = await _handler.GetMyComplaintsAsync(customerId.Value);
        return Ok(result);
    }

    /// <summary>Customer/Admin: get a single complaint by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _handler.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Complaint not found" });

        // Customer can only see their own
        if (User.IsInRole("Customer"))
        {
            var customerId = GetUserId();
            if (result.CustomerId != customerId)
                return Forbid();
        }

        return Ok(result);
    }

    /// <summary>Admin/Dealer: view all complaints.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? category = null)
    {
        var all = await _handler.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(status))
            all = all.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(category))
            all = all.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(new
        {
            total = all.Count,
            open = all.Count(c => c.Status == "Open"),
            inProgress = all.Count(c => c.Status == "InProgress"),
            resolved = all.Count(c => c.Status is "Resolved" or "Closed"),
            complaints = all
        });
    }

    /// <summary>Admin: update complaint status / add resolution note.</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateComplaintStatusRequest request)
    {
        try
        {
            var result = await _handler.UpdateStatusAsync(id, request);
            _logger.LogInformation("Complaint {Id} status updated to {Status}", id, request.Status);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var g) ? g : null;
    }
}
