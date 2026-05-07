namespace IFMS.Booking.Application.Interfaces;

public record KycProviderInput(
    string DocumentType,
    string? NormalizedPan,
    string? NormalizedAadhaarDigits,
    string? FullName);

public record KycProviderOutcome(
    bool IsVerified,
    string? FailureReason,
    string? ReferenceId,
    IReadOnlyList<string>? RiskFlags
);

/// <summary>
/// Government-grade verification via licensed provider (Setu, Signzy, etc.). Stub implementation for dev.
/// </summary>
public interface IKycVerificationProvider
{
    Task<KycProviderOutcome> VerifyAsync(KycProviderInput input, CancellationToken cancellationToken = default);
}
