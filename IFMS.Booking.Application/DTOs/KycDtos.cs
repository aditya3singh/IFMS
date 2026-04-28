namespace IFMS.Booking.Application.DTOs;

/// <param name="DocumentType">Use <c>Pan</c> or <c>Aadhaar</c> (case-insensitive). Only the matching field is required.</param>
public record VerifyKycRequest(
    string DocumentType,
    string? Pan = null,
    string? Aadhaar = null,
    string? FullName = null
);

public record VerifyKycResponse(
    bool Verified,
    string? Message,
    string? SessionId,
    string? ReferenceId,
    IReadOnlyList<string>? RiskFlags
);
