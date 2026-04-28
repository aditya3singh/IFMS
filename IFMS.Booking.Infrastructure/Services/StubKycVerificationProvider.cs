using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Application.Options;
using IFMS.Booking.Application.Support;
using Microsoft.Extensions.Options;

namespace IFMS.Booking.Infrastructure.Services;

/// <summary>
/// Simulates a licensed KYC provider. Swap for HTTP client to Setu / Signzy / Razorpay Identity, etc.
/// Does not call government APIs. Never log raw Aadhaar in real integrations.
/// </summary>
public class StubKycVerificationProvider : IKycVerificationProvider
{
    private readonly KycOptions _options;

    public StubKycVerificationProvider(IOptions<KycOptions> options) => _options = options.Value;

    public Task<KycProviderOutcome> VerifyAsync(KycProviderInput input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rejectPan = _options.StubRejectPan;
        if (string.Equals(input.DocumentType, "Pan", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(input.NormalizedPan) &&
            !string.IsNullOrWhiteSpace(rejectPan) &&
            string.Equals(input.NormalizedPan, KycFormatValidator.NormalizePan(rejectPan), StringComparison.Ordinal))
        {
            return Task.FromResult(new KycProviderOutcome(
                false,
                "Identity verification failed: this PAN is flagged for review (demo stub). Use another PAN to continue.",
                null,
                new[] { "SIMULATED_FRAUD_PAN" }));
        }

        // Real provider: call NSDL for PAN, UIDAI OTP / DigiLocker for Aadhaar; compare name; return provider reference id only.
        var referenceId = $"stub-{Guid.NewGuid():N}";
        return Task.FromResult(new KycProviderOutcome(true, null, referenceId, null));
    }
}
