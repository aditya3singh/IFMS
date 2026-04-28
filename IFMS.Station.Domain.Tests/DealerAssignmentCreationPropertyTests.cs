using FsCheck;
using FsCheck.Xunit;
using Xunit;
using IFMS.Station.Domain.Entities;

namespace IFMS.Station.Domain.Tests;

/// <summary>
/// Property-based tests for DealerAssignment creation
/// **Validates: Requirements 11.4, 11.5, 11.6**
/// </summary>
public class DealerAssignmentCreationPropertyTests
{
    /// <summary>
    /// Property 11: Dealer Assignment Creation Initializes All Fields
    /// 
    /// For any valid station ID and user ID where no assignment exists, creating a dealer 
    /// assignment should result in a new DealerAssignment entity with a non-empty Guid ID, 
    /// the provided StationId and UserId, and AssignedAt set to the current timestamp.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "station-management-service")]
    [Trait("Property", "Property 11: Dealer Assignment Creation Initializes All Fields")]
    public void DealerAssignmentCreation_InitializesAllFields(Guid stationId, Guid userId)
    {
        // Skip if either Guid is empty
        if (stationId == Guid.Empty || userId == Guid.Empty)
        {
            return;
        }
        
        // Record the time before creation
        var beforeCreation = DateTime.UtcNow;
        
        // Act: Create the dealer assignment
        var assignment = DealerAssignment.Create(stationId, userId);
        
        // Record the time after creation
        var afterCreation = DateTime.UtcNow;
        
        // Assert: Verify all required fields are initialized correctly
        Assert.NotEqual(Guid.Empty, assignment.Id);
        Assert.Equal(stationId, assignment.StationId);
        Assert.Equal(userId, assignment.UserId);
        Assert.InRange(assignment.AssignedAt, beforeCreation, afterCreation);
    }
}
