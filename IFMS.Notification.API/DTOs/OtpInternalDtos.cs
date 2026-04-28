namespace IFMS.Notification.API.DTOs;

public record OtpInternalRequest(
    string? Email,
    string? PhoneDigits,
    string Code,
    string Kind
);
