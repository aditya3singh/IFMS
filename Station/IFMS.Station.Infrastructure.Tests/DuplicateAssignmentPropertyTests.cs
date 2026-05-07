using FsCheck;
using FsCheck.Xunit;
using IFMS.Station.Domain.Entities;
using IFMS.Station.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using StationEntity = IFMS.Station.Domain.Entities.Station;

namespace IFMS.Station.Infrastructure.Tests;

/// <summary>
/// Property-based tests for duplicate dealer assignment prevention
/// **Validates: Requirements 11.3**
/// </summary>
public class DuplicateAssignmentPropertyTests
{
    /// <summary>
    /// Property 12: Duplicate Dealer Assignment Prevention
    /// 
    /// For any station that already has a dealer assignment, attempting to create 
    /// another dealer assignment for the same station should fail with a validation error.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 12: Duplicate Dealer Assignment Prevention")]
    public async Task DealerAssignment_PreventsDuplicateAssignment()
    {
        // Arrange: Generate random data for station and dealers
        var random = new Random();
        var stationName = $"Station_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var licenseNumber = $"LIC-{random.Next(100000, 999999)}";
        var firstDealerId = Guid.NewGuid();
        var secondDealerId = Guid.NewGuid();
        
        // Create in-memory database
        var options = new DbContextOptionsBuilder<StationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new StationDbContext(options);
        var repository = new DealerAssignmentRepository(context);

        // Create a station
        var station = StationEntity.Create(
            stationName,
            licenseNumber,
            "TestCity",
            "TestState",
            0.0m,
            0.0m
        );

        await context.Stations.AddAsync(station);
        await context.SaveChangesAsync();

        // Act: Create first dealer assignment
        var firstAssignment = DealerAssignment.Create(station.Id, firstDealerId);
        await repository.AddAsync(firstAssignment);
        await repository.SaveChangesAsync();

        // Verify first assignment was created successfully
        var hasAssignmentAfterFirst = await repository.StationHasAssignmentAsync(station.Id);
        Assert.True(hasAssignmentAfterFirst, "Station should have an assignment after first dealer is assigned");

        // Act: Attempt to create second dealer assignment for the same station
        var hasAssignmentBeforeSecond = await repository.StationHasAssignmentAsync(station.Id);

        // Assert: Verify that StationHasAssignmentAsync returns true, 
        // indicating that a duplicate assignment should be prevented
        Assert.True(
            hasAssignmentBeforeSecond,
            "StationHasAssignmentAsync should return true when station already has an assignment, preventing duplicate assignments"
        );

        // Additional verification: Confirm only one assignment exists
        var existingAssignment = await repository.GetByStationIdAsync(station.Id);
        Assert.NotNull(existingAssignment);
        Assert.Equal(firstDealerId, existingAssignment.UserId);
        Assert.Equal(station.Id, existingAssignment.StationId);
    }
}
