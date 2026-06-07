using IFMS.Inventory.Application.Commands;
using IFMS.Inventory.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IFMS.Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Dealer")]
public class SuppliersController : ControllerBase
{
    private readonly SupplierCommandHandler _handler;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(
        SupplierCommandHandler handler,
        ILogger<SuppliersController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _handler.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return StatusCode(500, new { error = "Failed to retrieve suppliers" });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var result = await _handler.GetActiveAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active suppliers");
            return StatusCode(500, new { error = "Failed to retrieve suppliers" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _handler.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { error = "Supplier not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier {SupplierId}", id);
            return StatusCode(500, new { error = "Failed to retrieve supplier" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        try
        {
            var result = await _handler.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return StatusCode(500, new { error = "Failed to create supplier" });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request)
    {
        try
        {
            var result = await _handler.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {SupplierId}", id);
            return StatusCode(500, new { error = "Failed to update supplier" });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSupplierStatusRequest request)
    {
        try
        {
            var result = await _handler.UpdateStatusAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier status {SupplierId}", id);
            return StatusCode(500, new { error = "Failed to update supplier status" });
        }
    }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {SupplierId}", id);
            return StatusCode(500, new { error = "Failed to delete supplier" });
        }
    }
}
