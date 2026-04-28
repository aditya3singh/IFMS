using IFMS.Booking.Application.DTOs;
using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Application.Options;
using IFMS.Booking.Application.Support;
using Microsoft.Extensions.Options;

namespace IFMS.Booking.Application.Commands;

public class KycVerificationHandler
{
    private readonly IKycVerificationProvider _provider;
    private readonly IKycSessionStore _sessions;
    private readonly IOptions<KycOptions> _kycOptions;

    public KycVerificationHandler(
        IKycVerificationProvider provider,
        IKycSessionStore sessions,
        IOptions<KycOptions> kycOptions)
    {
        _provider = provider;
        _sessions = sessions;
        _kycOptions = kycOptions;
    }

    public async Task<VerifyKycResponse> VerifyForBookingAsync(Guid customerId, VerifyKycRequest request, CancellationToken cancellationToken = default)
    {
        var doc = (request.DocumentType ?? string.Empty).Trim();
        string? pan = null;
        string? aadhaarDigits = null;

        if (string.Equals(doc, "Pan", StringComparison.OrdinalIgnoreCase))
        {
            pan = KycFormatValidator.NormalizePan(request.Pan ?? string.Empty);
            if (!KycFormatValidator.IsValidPanFormat(pan))
            {
                return new VerifyKycResponse(false, "Invalid PAN format (expect 5 letters, 4 digits, 1 letter).", null, null,
                    new[] { "INVALID_PAN_FORMAT" });
            }
        }
        else if (string.Equals(doc, "Aadhaar", StringComparison.OrdinalIgnoreCase))
        {
            aadhaarDigits = KycFormatValidator.CleanAadhaarDigits(request.Aadhaar ?? string.Empty);
            if (!KycFormatValidator.IsValidAadhaarFormat(aadhaarDigits))
            {
                return new VerifyKycResponse(false,
                    "Invalid Aadhaar number (12 digits, cannot start with 0 or 1, checksum must be valid).", null, null,
                    new[] { "INVALID_AADHAAR_FORMAT" });
            }
        }
        else
        {
            return new VerifyKycResponse(false,
                "Select document type: Pan or Aadhaar.",
                null,
                null,
                new[] { "INVALID_DOCUMENT_TYPE" });
        }

        var outcome = await _provider.VerifyAsync(
            new KycProviderInput(doc, pan, aadhaarDigits, request.FullName?.Trim()),
            cancellationToken);

        if (!outcome.IsVerified)
        {
            return new VerifyKycResponse(false,
                outcome.FailureReason ?? "Identity could not be verified.",
                null,
                null,
                outcome.RiskFlags);
        }

        var ttl = TimeSpan.FromMinutes(Math.Clamp(_kycOptions.Value.SessionTtlMinutes, 5, 60));
        var referenceId = outcome.ReferenceId ?? $"ref-{Guid.NewGuid():N}";
        var sessionId = await _sessions.CreateSessionAsync(customerId, referenceId, ttl, cancellationToken);

        return new VerifyKycResponse(true, null, sessionId, referenceId, null);
    }
}
