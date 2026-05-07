namespace IFMS.Station.Domain.Entities;

public class DealerAssignment
{
    public Guid Id { get; private set; }
    public Guid StationId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    
    // Navigation property
    public Station Station { get; private set; } = null!;
    
    private DealerAssignment() { }
    
    public static DealerAssignment Create(Guid stationId, Guid userId)
    {
        return new DealerAssignment
        {
            Id = Guid.NewGuid(),
            StationId = stationId,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
