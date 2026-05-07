using IFMS.Station.Application.DTOs;
using IFMS.Station.Application.Interfaces;
using IFMS.Station.Application.Pricing;
using IFMS.Station.Application.Services;
using IFMS.Station.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IFMS.Station.API.Controllers;

[ApiController]
[Route("api/Stations")]
[Authorize]
public class StationsController : ControllerBase
{
    private readonly IStationRepository _stationRepository;
    private readonly IDealerAssignmentRepository _dealerAssignmentRepository;
    private readonly IFuelPriceService _fuelPriceService;
    private readonly ILogger<StationsController> _logger;
    
    public StationsController(
        IStationRepository stationRepository,
        IDealerAssignmentRepository dealerAssignmentRepository,
        IFuelPriceService fuelPriceService,
        ILogger<StationsController> logger)
    {
        _stationRepository = stationRepository;
        _dealerAssignmentRepository = dealerAssignmentRepository;
        _fuelPriceService = fuelPriceService;
        _logger = logger;
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStation([FromBody] CreateStationDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new station with license number {LicenseNumber}", dto.LicenseNumber);
            
            // Check license number uniqueness
            if (await _stationRepository.LicenseNumberExistsAsync(dto.LicenseNumber))
            {
                _logger.LogWarning("License number {LicenseNumber} already exists", dto.LicenseNumber);
                return BadRequest(new { error = "License number already exists" });
            }
            
            // Create station entity
            var station = Domain.Entities.Station.Create(
                dto.Name,
                dto.LicenseNumber,
                dto.City,
                dto.State,
                dto.Latitude,
                dto.Longitude
            );
            
            await _stationRepository.AddAsync(station);
            await _stationRepository.SaveChangesAsync();
            
            _logger.LogInformation("Station created successfully with ID {StationId}", station.Id);
            
            var response = MapToResponseDto(station);
            return CreatedAtAction(nameof(GetStationById), new { id = station.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating station");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating station");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    [HttpGet]
    [AllowAnonymous] // Station list is needed to show the booking form — safe read-only endpoint
    public async Task<IActionResult> GetAllStations()
    {
        try
        {
            _logger.LogInformation("Retrieving all active stations");
            
            var stations = await _stationRepository.GetAllAsync();
            var response = stations.Select(MapToResponseDto).ToList();
            
            _logger.LogInformation("Retrieved {Count} active stations", response.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stations");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    /// <summary>Retail unit price by station area (state + city) and fuel type. Read-only; used by customer booking UI.</summary>
    /// <remarks>Order -1000: literal segment wins over <c>{id:guid}</c> on hosts running older builds.</remarks>
    [HttpGet("fuel-price-quote", Order = -1000)]
    [AllowAnonymous]
    public IActionResult GetFuelPriceQuote([FromQuery] string? state, [FromQuery] string? city, [FromQuery] string? fuelType)
    {
        var q = RegionalFuelPriceQuote.GetQuote(state, city, fuelType);
        var dto = new FuelPriceQuoteDto(q.PricePerUnit, q.UnitLabel, q.AreaSummary, DateTimeOffset.UtcNow);
        return Ok(dto);
    }

    /// <summary>Retail quote using this station’s stored city and state (authoritative for booking).</summary>
    [HttpGet("{id:guid}/fuel-price-quote")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFuelPriceQuoteForStation(Guid id, [FromQuery] string? fuelType)
    {
        var station = await _stationRepository.GetByIdAsync(id);
        if (station == null)
        {
            _logger.LogWarning("Fuel quote requested for unknown station {StationId}", id);
            return NotFound(new { error = "Station not found" });
        }

        var q = RegionalFuelPriceQuote.GetQuote(station.State, station.City, fuelType);
        var dto = new FuelPriceQuoteDto(q.PricePerUnit, q.UnitLabel, q.AreaSummary, DateTimeOffset.UtcNow);
        return Ok(dto);
    }

    /// <summary>Admin: all active stations. Dealer: only stations assigned to the signed-in user.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetMyStations()
    {
        try
        {
            if (User.IsInRole("Admin"))
            {
                var all = await _stationRepository.GetAllAsync();
                return Ok(all.Select(MapToResponseDto).ToList());
            }

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { error = "Invalid identity" });

            var ids = await _dealerAssignmentRepository.GetStationIdsForUserAsync(userId.Value);
            var stations = await _stationRepository.GetByIdsAsync(ids);
            return Ok(stations.Select(MapToResponseDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my stations");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    /// <remarks><c>{id:guid}</c> avoids matching literal paths like <c>fuel-price-quote</c> (would otherwise fail Guid binding → 400).</remarks>
    [HttpGet("{id:guid}")]
    [AllowAnonymous] // Station detail is needed for booking display — safe read-only endpoint
    public async Task<IActionResult> GetStationById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving station with ID {StationId}", id);
            
            var station = await _stationRepository.GetByIdAsync(id);
            
            if (station == null)
            {
                _logger.LogWarning("Station with ID {StationId} not found", id);
                return NotFound(new { error = "Station not found" });
            }
            
            var response = MapToResponseDto(station);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving station with ID {StationId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> UpdateStation(Guid id, [FromBody] UpdateStationDto dto)
    {
        try
        {
            _logger.LogInformation("Updating station with ID {StationId}", id);

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { error = "Invalid identity" });
                if (!await _dealerAssignmentRepository.UserIsAssignedToStationAsync(userId.Value, id))
                    return Forbid();
            }
            
            var station = await _stationRepository.GetByIdAsync(id);
            
            if (station == null)
            {
                _logger.LogWarning("Station with ID {StationId} not found for update", id);
                return NotFound(new { error = "Station not found" });
            }
            
            // Check license number uniqueness (excluding current station)
            if (await _stationRepository.LicenseNumberExistsAsync(dto.LicenseNumber, id))
            {
                _logger.LogWarning("License number {LicenseNumber} already exists on another station", dto.LicenseNumber);
                return BadRequest(new { error = "License number already exists" });
            }
            
            // Update station
            station.Update(
                dto.Name,
                dto.LicenseNumber,
                dto.City,
                dto.State,
                dto.Latitude,
                dto.Longitude
            );
            
            await _stationRepository.UpdateAsync(station);
            await _stationRepository.SaveChangesAsync();
            
            _logger.LogInformation("Station with ID {StationId} updated successfully", id);
            
            var response = MapToResponseDto(station);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating station with ID {StationId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating station with ID {StationId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStation(Guid id)
    {
        try
        {
            _logger.LogInformation("Soft deleting station with ID {StationId}", id);
            
            var station = await _stationRepository.GetByIdAsync(id);
            
            if (station == null)
            {
                _logger.LogWarning("Station with ID {StationId} not found for deletion", id);
                return NotFound(new { error = "Station not found" });
            }
            
            station.SoftDelete();
            await _stationRepository.UpdateAsync(station);
            await _stationRepository.SaveChangesAsync();
            
            _logger.LogInformation("Station with ID {StationId} soft deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting station with ID {StationId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    [HttpPost("{id:guid}/assign-dealer")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignDealer(Guid id, [FromBody] AssignDealerDto dto)
    {
        try
        {
            _logger.LogInformation("Assigning dealer {UserId} to station {StationId}", dto.UserId, id);
            
            // Validate UserId is not empty
            if (dto.UserId == Guid.Empty)
            {
                _logger.LogWarning("Empty UserId provided for dealer assignment");
                return BadRequest(new { error = "UserId is required" });
            }
            
            // Verify station exists and is active
            var station = await _stationRepository.GetByIdAsync(id);
            
            if (station == null)
            {
                _logger.LogWarning("Station with ID {StationId} not found for dealer assignment", id);
                return NotFound(new { error = "Station not found" });
            }
            
            // Upsert: remove existing assignment first so admin can always reassign
            var existing = await _dealerAssignmentRepository.GetByStationIdAsync(id);
            if (existing != null)
            {
                _logger.LogInformation("Station {StationId} already has dealer {OldUserId} — removing before reassignment", id, existing.UserId);
                await _dealerAssignmentRepository.RemoveAsync(existing);
                await _dealerAssignmentRepository.SaveChangesAsync();
            }

            // Enforce one dealer -> one station mapping.
            // If this dealer is linked elsewhere, detach old links before assigning.
            var existingForDealer = await _dealerAssignmentRepository.GetByUserIdAsync(dto.UserId);
            foreach (var row in existingForDealer.Where(x => x.StationId != id))
            {
                _logger.LogInformation(
                    "Dealer {UserId} already assigned to station {OldStationId} — removing old assignment",
                    dto.UserId,
                    row.StationId);
                await _dealerAssignmentRepository.RemoveAsync(row);
            }
            if (existingForDealer.Any(x => x.StationId != id))
            {
                await _dealerAssignmentRepository.SaveChangesAsync();
            }

            // Create dealer assignment
            var assignment = DealerAssignment.Create(id, dto.UserId);
            
            await _dealerAssignmentRepository.AddAsync(assignment);
            await _dealerAssignmentRepository.SaveChangesAsync();
            
            _logger.LogInformation("Dealer {UserId} assigned to station {StationId} successfully", dto.UserId, id);
            
            var response = new DealerAssignmentResponseDto(
                assignment.Id,
                assignment.StationId,
                assignment.UserId,
                assignment.AssignedAt
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning dealer to station {StationId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    [HttpGet("{id:guid}/dealer")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<IActionResult> GetDealerAssignment(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving dealer assignment for station {StationId}", id);

            if (User.IsInRole("Dealer"))
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { error = "Invalid identity" });
                if (!await _dealerAssignmentRepository.UserIsAssignedToStationAsync(userId.Value, id))
                    return Forbid();
            }
            
            var assignment = await _dealerAssignmentRepository.GetByStationIdAsync(id);
            
            if (assignment == null)
            {
                _logger.LogWarning("No dealer assignment found for station {StationId}", id);
                return NotFound(new { error = "No dealer assigned to this station" });
            }
            
            var response = new DealerAssignmentResponseDto(
                assignment.Id,
                assignment.StationId,
                assignment.UserId,
                assignment.AssignedAt
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dealer assignment for station {StationId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
    
    /// <summary>Admin: unassign the current dealer from a station.</summary>
    [HttpDelete("{id:guid}/dealer")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignDealer(Guid id)
    {
        try
        {
            var assignment = await _dealerAssignmentRepository.GetByStationIdAsync(id);
            if (assignment == null)
                return NotFound(new { error = "No dealer is assigned to this station." });

            await _dealerAssignmentRepository.RemoveAsync(assignment);
            await _dealerAssignmentRepository.SaveChangesAsync();

            _logger.LogInformation("Dealer unassigned from station {StationId}", id);
            return Ok(new { message = "Dealer unassigned successfully.", stationId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning dealer from station {StationId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    /// <summary>Search/filter stations by name, city, or state. Public.</summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchStations(
        [FromQuery] string? q = null,
        [FromQuery] string? city = null,
        [FromQuery] string? state = null)
    {
        try
        {
            var stations = await _stationRepository.GetAllAsync();
            var filtered = stations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(q))
                filtered = filtered.Where(s =>
                    s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    s.LicenseNumber.Contains(q, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(city))
                filtered = filtered.Where(s => s.City.Equals(city, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(state))
                filtered = filtered.Where(s => s.State.Equals(state, StringComparison.OrdinalIgnoreCase));

            var result = filtered.Select(MapToResponseDto).ToList();
            return Ok(new { count = result.Count, stations = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stations");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    /// <summary>Find stations near a coordinate within a radius (km). Public.</summary>
    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNearbyStations(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 10)
    {
        try
        {
            var stations = await _stationRepository.GetAllAsync();

            var nearby = stations
                .Select(s => new
                {
                    station = s,
                    distanceKm = HaversineKm(lat, lng, (double)s.Latitude, (double)s.Longitude)
                })
                .Where(x => x.distanceKm <= radiusKm)
                .OrderBy(x => x.distanceKm)
                .Select(x => new
                {
                    station = MapToResponseDto(x.station),
                    distanceKm = Math.Round(x.distanceKm, 2)
                })
                .ToList();

            return Ok(new { lat, lng, radiusKm, count = nearby.Count, stations = nearby });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding nearby stations");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    private Guid? GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var g) ? g : null;
    }

    private static StationResponseDto MapToResponseDto(Domain.Entities.Station station)
    {
        DealerAssignmentResponseDto? dealerAssignment = null;
        
        if (station.DealerAssignment != null)
        {
            dealerAssignment = new DealerAssignmentResponseDto(
                station.DealerAssignment.Id,
                station.DealerAssignment.StationId,
                station.DealerAssignment.UserId,
                station.DealerAssignment.AssignedAt
            );
        }
        
        return new StationResponseDto(
            station.Id,
            station.Name,
            station.LicenseNumber,
            station.City,
            station.State,
            station.Latitude,
            station.Longitude,
            station.IsActive,
            station.CreatedAt,
            station.UpdatedAt,
            dealerAssignment
        );
    }

    /// <summary>Haversine formula — great-circle distance in km.</summary>
    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    /// <summary>Get real-time fuel prices from Indian fuel price API for a specific location</summary>
    [HttpGet("realtime-price")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRealtimeFuelPrice(
        [FromQuery] string state,
        [FromQuery] string district,
        [FromQuery] string? fuelType = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(district))
            {
                return BadRequest(new { error = "State and district are required" });
            }

            var priceData = await _fuelPriceService.GetFuelPriceAsync(state, district, fuelType ?? "Petrol");
            
            if (priceData == null)
            {
                return NotFound(new { error = "Fuel price data not available for this location" });
            }

            return Ok(new
            {
                state = priceData.State,
                district = priceData.District,
                prices = new
                {
                    petrol = priceData.PetrolPrice,
                    diesel = priceData.DieselPrice,
                    cng = priceData.CngPrice
                },
                currency = priceData.Currency,
                fetchedAt = priceData.FetchedAt,
                source = priceData.Source,
                note = "Prices are updated daily at 6:00 AM IST"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching realtime fuel price");
            return StatusCode(500, new { error = "Failed to fetch fuel prices" });
        }
    }

    /// <summary>Get real-time fuel prices for a station based on its location</summary>
    [HttpGet("{id:guid}/realtime-price")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStationRealtimePrice(Guid id)
    {
        try
        {
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound(new { error = "Station not found" });
            }

            var priceData = await _fuelPriceService.GetFuelPriceAsync(
                station.State,
                station.City,
                "Petrol");
            
            if (priceData == null)
            {
                return NotFound(new { error = "Fuel price data not available for this station" });
            }

            return Ok(new
            {
                stationId = station.Id,
                stationName = station.Name,
                location = new
                {
                    state = station.State,
                    city = station.City
                },
                prices = new
                {
                    petrol = priceData.PetrolPrice,
                    diesel = priceData.DieselPrice,
                    cng = priceData.CngPrice
                },
                currency = priceData.Currency,
                fetchedAt = priceData.FetchedAt,
                source = priceData.Source,
                note = "Prices are updated daily at 6:00 AM IST"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching station realtime price");
            return StatusCode(500, new { error = "Failed to fetch fuel prices" });
        }
    }

    /// <summary>Get all available states for fuel price lookup</summary>
    [HttpGet("available-states")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableStates()
    {
        try
        {
            var states = await _fuelPriceService.GetAvailableStatesAsync();
            return Ok(new { states, count = states.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available states");
            return StatusCode(500, new { error = "Failed to fetch states" });
        }
    }

    /// <summary>Get all districts in a state</summary>
    [HttpGet("districts/{state}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDistricts(string state)
    {
        try
        {
            var districts = await _fuelPriceService.GetDistrictsAsync(state);
            return Ok(new { state, districts, count = districts.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching districts");
            return StatusCode(500, new { error = "Failed to fetch districts" });
        }
    }

    /// <summary>Get all fuel prices for a state</summary>
    [HttpGet("state-prices/{state}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatePrices(string state)
    {
        try
        {
            var prices = await _fuelPriceService.GetStateFuelPricesAsync(state);
            return Ok(new
            {
                state,
                districts = prices.Select(p => new
                {
                    district = p.District,
                    prices = new
                    {
                        petrol = p.PetrolPrice,
                        diesel = p.DieselPrice,
                        cng = p.CngPrice
                    },
                    currency = p.Currency
                }),
                count = prices.Count,
                fetchedAt = prices.FirstOrDefault()?.FetchedAt ?? DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching state prices");
            return StatusCode(500, new { error = "Failed to fetch state prices" });
        }
    }
}
