using FsCheck;
using FsCheck.Xunit;
using IFMS.Station.Application.DTOs;
using IFMS.Station.Application.Interfaces;
using IFMS.Station.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;
using StationsController = IFMS.Station.API.Controllers.StationsController;

namespace IFMS.Station.API.Tests;

/// <summary>
/// Property-based tests for license number uniqueness enforcement
/// **Validates: Requirements 6.7, 9.7**
/// </summary>
public class LicenseNumberUniquenessPropertyTests
{
    /// <summary>
    /// Property 4: License Number Uniqueness Enforcement
    /// 
    /// For any station creation or update request with a license number that already exists 
    /// on a different station, the operation should fail with a validation error indicating 
    /// the license number must be unique.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 4: License Number Uniqueness Enforcement")]
    public async Task CreateStation_RejectsDuplicateLicenseNumber()
    {
        // Arrange: Create in-memory database with a station
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var stationRepository = new StationRepository(context);
        var dealerAssignmentRepository = new DealerAssignmentRepository(context);
        var logger = Mock.Of<ILogger<StationsController>>();
        var controller = new StationsController(stationRepository, dealerAssignmentRepository, logger);

        // Create an existing station with a specific license number
        var existingLicenseNumber = $"LIC-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var existingStation = StationEntity.Create(
            "Existing Station",
            existingLicenseNumber,
            "Mumbai",
            "Maharashtra",
            19.076090m,
            72.877426m
        );

        await context.Stations.AddAsync(existingStation);
        await context.SaveChangesAsync();

        // Act: Try to create a new station with the same license number
        var duplicateDto = new CreateStationDto(
            "New Station",
            existingLicenseNumber, // Same license number
            "Delhi",
            "Delhi",
            28.704060m,
            77.102493m
        );

        var result = await controller.CreateStation(duplicateDto);

        // Assert: Verify the operation fails with BadRequest
        var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        // Verify the error message indicates license number already exists
        var errorResponse = badRequestResult.Value;
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.NotNull(errorProperty);
        
        var errorMessage = errorProperty.GetValue(errorResponse)?.ToString();
        Assert.NotNull(errorMessage);
        Assert.Contains("License number", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exists", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 4: License Number Uniqueness Enforcement")]
    public async Task UpdateStation_RejectsDuplicateLicenseNumber()
    {
        // Arrange: Create in-memory database with two stations
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var stationRepository = new StationRepository(context);
        var dealerAssignmentRepository = new DealerAssignmentRepository(context);
        var logger = Mock.Of<ILogger<StationsController>>();
        var controller = new StationsController(stationRepository, dealerAssignmentRepository, logger);

        // Create first station with license number A
        var licenseNumberA = $"LIC-A-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var stationA = StationEntity.Create(
            "Station A",
            licenseNumberA,
            "Mumbai",
            "Maharashtra",
            19.076090m,
            72.877426m
        );

        // Create second station with license number B
        var licenseNumberB = $"LIC-B-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var stationB = StationEntity.Create(
            "Station B",
            licenseNumberB,
            "Delhi",
            "Delhi",
            28.704060m,
            77.102493m
        );

        await context.Stations.AddAsync(stationA);
        await context.Stations.AddAsync(stationB);
        await context.SaveChangesAsync();

        // Act: Try to update Station B to use Station A's license number
        var updateDto = new UpdateStationDto(
            "Station B Updated",
            licenseNumberA, // Trying to use Station A's license number
            "Delhi",
            "Delhi",
            28.704060m,
            77.102493m
        );

        var result = await controller.UpdateStation(stationB.Id, updateDto);

        // Assert: Verify the operation fails with BadRequest
        var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        // Verify the error message indicates license number already exists
        var errorResponse = badRequestResult.Value;
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.NotNull(errorProperty);
        
        var errorMessage = errorProperty.GetValue(errorResponse)?.ToString();
        Assert.NotNull(errorMessage);
        Assert.Contains("License number", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exists", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 4: License Number Uniqueness Enforcement")]
    public async Task UpdateStation_AllowsSameLicenseNumberForSameStation()
    {
        // Arrange: Create in-memory database with a station
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var stationRepository = new StationRepository(context);
        var dealerAssignmentRepository = new DealerAssignmentRepository(context);
        var logger = Mock.Of<ILogger<StationsController>>();
        var controller = new StationsController(stationRepository, dealerAssignmentRepository, logger);

        // Create a station
        var licenseNumber = $"LIC-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var station = StationEntity.Create(
            "Original Station",
            licenseNumber,
            "Mumbai",
            "Maharashtra",
            19.076090m,
            72.877426m
        );

        await context.Stations.AddAsync(station);
        await context.SaveChangesAsync();

        // Act: Update the station with the same license number (should be allowed)
        var updateDto = new UpdateStationDto(
            "Updated Station Name",
            licenseNumber, // Same license number
            "Mumbai",
            "Maharashtra",
            19.076090m,
            72.877426m
        );

        var result = await controller.UpdateStation(station.Id, updateDto);

        // Assert: Verify the operation succeeds
        var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Verify the station was updated
        var response = Assert.IsType<StationResponseDto>(okResult.Value);
        Assert.Equal("Updated Station Name", response.Name);
        Assert.Equal(licenseNumber, response.LicenseNumber);
    }

    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 4: License Number Uniqueness Enforcement")]
    public async Task CreateStation_AllowsUniqueLicenseNumber()
    {
        // Arrange: Create in-memory database with a station
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var stationRepository = new StationRepository(context);
        var dealerAssignmentRepository = new DealerAssignmentRepository(context);
        var logger = Mock.Of<ILogger<StationsController>>();
        var controller = new StationsController(stationRepository, dealerAssignmentRepository, logger);

        // Create an existing station
        var existingLicenseNumber = $"LIC-EXISTING-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var existingStation = StationEntity.Create(
            "Existing Station",
            existingLicenseNumber,
            "Mumbai",
            "Maharashtra",
            19.076090m,
            72.877426m
        );

        await context.Stations.AddAsync(existingStation);
        await context.SaveChangesAsync();

        // Act: Create a new station with a unique license number
        var uniqueLicenseNumber = $"LIC-UNIQUE-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var newDto = new CreateStationDto(
            "New Station",
            uniqueLicenseNumber, // Unique license number
            "Delhi",
            "Delhi",
            28.704060m,
            77.102493m
        );

        var result = await controller.CreateStation(newDto);

        // Assert: Verify the operation succeeds
        var createdResult = Assert.IsType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>(result);
        Assert.NotNull(createdResult.Value);
        
        // Verify the station was created with the unique license number
        var response = Assert.IsType<StationResponseDto>(createdResult.Value);
        Assert.Equal(uniqueLicenseNumber, response.LicenseNumber);
        Assert.Equal("New Station", response.Name);
    }
}
