using FsCheck;
using FsCheck.Xunit;
using IFMS.Station.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;

namespace IFMS.Station.Infrastructure.Tests;

/// <summary>
/// Property-based tests for GetAll repository behavior
/// **Validates: Requirements 7.1, 7.2, 7.3**
/// </summary>
public class GetAllStationsPropertyTests
{
    /// <summary>
    /// Property 8: GetAll Returns Only Active Stations Sorted By Name
    /// 
    /// For any database state containing both active and inactive stations, 
    /// calling GetAll should return only stations where IsActive is true, 
    /// and the results should be ordered by Name in ascending alphabetical order.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 8: GetAll Returns Only Active Stations Sorted By Name")]
    public async Task GetAllAsync_ReturnsOnlyActiveStationsSortedByName()
    {
        // Arrange: Generate random station data with mixed IsActive states
        var random = new Random();
        var stationNames = new[] 
        { 
            "Alpha Station", "Beta Station", "Charlie Station", "Delta Station", 
            "Echo Station", "Foxtrot Station", "Golf Station", "Hotel Station",
            "India Station", "Juliet Station", "Kilo Station", "Lima Station"
        };

        var stationCount = random.Next(5, 15);
        var stationData = new List<(string name, bool isActive, int seed)>();
        
        for (int i = 0; i < stationCount; i++)
        {
            var name = stationNames[random.Next(stationNames.Length)];
            var isActive = random.Next(2) == 0; // Randomly true or false
            var seed = random.Next(1000000);
            stationData.Add((name, isActive, seed));
        }

        // Create in-memory database
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var repository = new StationRepository(context);

        // Create stations with mixed IsActive states
        var expectedActiveStations = new List<StationEntity>();

        foreach (var (name, isActive, seed) in stationData)
        {
            var licenseNumber = $"LIC-{seed}";
            var station = StationEntity.Create(
                name,
                licenseNumber,
                "TestCity",
                "TestState",
                0.0m,
                0.0m
            );

            // Set IsActive state
            if (!isActive)
            {
                station.SoftDelete();
            }
            else
            {
                expectedActiveStations.Add(station);
            }

            await context.Stations.AddAsync(station);
        }

        await context.SaveChangesAsync();

        // Act: Call GetAllAsync
        var result = await repository.GetAllAsync();
        var resultList = result.ToList();

        // Assert: Verify only active stations are returned
        Assert.All(resultList, s => Assert.True(s.IsActive, "All returned stations should be active"));

        // Assert: Verify results are sorted by Name in ascending alphabetical order
        for (int i = 0; i < resultList.Count - 1; i++)
        {
            var comparison = string.Compare(resultList[i].Name, resultList[i + 1].Name, StringComparison.Ordinal);
            Assert.True(
                comparison <= 0,
                $"Stations should be sorted by Name in ascending order. Found '{resultList[i].Name}' before '{resultList[i + 1].Name}'"
            );
        }

        // Assert: Verify count matches expected active stations
        Assert.Equal(expectedActiveStations.Count, resultList.Count);

        // Assert: Verify all returned stations are in the expected set
        foreach (var station in resultList)
        {
            Assert.Contains(station.Id, expectedActiveStations.Select(e => e.Id));
        }
    }
}
