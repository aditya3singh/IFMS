namespace IFMS.Booking.Application.Interfaces;

public record KycSessionRecord(string ReferenceId);

/// <summary>Short-lived, one-time KYC session after successful verify (no PAN/Aadhaar stored here).</summary>
public interface IKycSessionStore
{
    Task<string> CreateSessionAsync(Guid customerId, string verificationReferenceId, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>Validates session belongs to customer and removes it (single use).</summary>
    Task<bool> TryConsumeAsync(string sessionId, Guid customerId, CancellationToken cancellationToken = default);
}
