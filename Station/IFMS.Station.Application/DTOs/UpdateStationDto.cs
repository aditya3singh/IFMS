namespace IFMS.Station.Application.DTOs;

public record UpdateStationDto(
    string Name,
    string LicenseNumber,
    string City,
    string State,
    decimal Latitude,
    decimal Longitude
);
