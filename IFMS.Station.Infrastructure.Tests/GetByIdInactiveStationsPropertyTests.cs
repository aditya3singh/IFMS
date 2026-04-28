using FsCheck;
using FsCheck.Xunit;
using IFMS.Station.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;

namespace IFMS.Station.Infrastructure.Tests;

/// <summary>
/// Property-based tests for GetById behavior with inactive stations
/// **Validates: Requirements 8.5**
/// </summary>
public class GetByIdInactiveStationsPropertyTests
{
    /// <summary>
    /// Property 9: GetById Excludes Inactive Stations
    /// 
    /// For any station that exists in the database but has IsActive set to false, 
    /// calling GetById with that station's ID should return null (treated as not found).
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 9: GetById Excludes Inactive Stations")]
    public async Task GetByIdAsync_ReturnsNullForInactiveStations()
    {
        // Arrange: Generate random station data
        var random = new Random();
        var seed = random.Next(1000000);
        
        var name = $"Station-{seed}";
        var licenseNumber = $"LIC-{seed}";
        var city = "TestCity";
        var state = "TestState";
        var latitude = (decimal)(random.NextDouble() * 180 - 90);
        var longitude = (decimal)(random.NextDouble() * 360 - 180);
        
        // Create in-memory database
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var repository = new StationRepository(context);

        // Create a station
        var station = StationEntity.Create(
            name,
            licenseNumber,
            city,
            state,
            latitude,
            longitude
        );

        await context.Stations.AddAsync(station);
        await context.SaveChangesAsync();
        
        // Verify station is initially active and can be retrieved
        var activeStation = await repository.GetByIdAsync(station.Id);
        Assert.NotNull(activeStation);
        Assert.True(activeStation.IsActive);
        
        // Act: Soft delete the station (set IsActive to false)
        station.SoftDelete();
        await context.SaveChangesAsync();
        
        // Assert: GetByIdAsync should return null for inactive station
        var inactiveStation = await repository.GetByIdAsync(station.Id);
        Assert.Null(inactiveStation);
        
        // Verify the station still exists in the database but is inactive
        var stationInDb = await context.Stations
            .FirstOrDefaultAsync(s => s.Id == station.Id);
        Assert.NotNull(stationInDb);
        Assert.False(stationInDb.IsActive);
    }
}
