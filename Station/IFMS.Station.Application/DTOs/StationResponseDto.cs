namespace IFMS.Station.Application.DTOs;

public record StationResponseDto(
    Guid Id,
    string Name,
    string LicenseNumber,
    string City,
    string State,
    decimal Latitude,
    decimal Longitude,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DealerAssignmentResponseDto? DealerAssignment
);
