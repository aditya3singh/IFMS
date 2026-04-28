namespace IFMS.Station.Application.DTOs;

public record DealerAssignmentResponseDto(
    Guid Id,
    Guid StationId,
    Guid UserId,
    DateTime AssignedAt
);
