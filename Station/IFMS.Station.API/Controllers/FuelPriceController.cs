using IFMS.Station.Application.DTOs;
using IFMS.Station.Application.Pricing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IFMS.Station.API.Controllers;

/// <summary>
/// Fuel retail quote by area — separate route prefix so this never collides with <c>/api/Stations/{id}</c> on any host.
/// </summary>
[ApiController]
[Route("api/fuel-price")]
[AllowAnonymous]
public class FuelPriceController : ControllerBase
{
    [HttpGet("quote")]
    public IActionResult GetQuote([FromQuery] string? state, [FromQuery] string? city, [FromQuery] string? fuelType)
    {
        var q = RegionalFuelPriceQuote.GetQuote(state, city, fuelType);
        var dto = new FuelPriceQuoteDto(q.PricePerUnit, q.UnitLabel, q.AreaSummary, DateTimeOffset.UtcNow);
        return Ok(dto);
    }
}
