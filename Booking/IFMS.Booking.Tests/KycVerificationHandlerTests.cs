using IFMS.Booking.Application.Commands;
using IFMS.Booking.Application.DTOs;
using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Application.Options;
using Microsoft.Extensions.Options;
using Moq;

namespace IFMS.Booking.Tests;

public class KycVerificationHandlerTests
{
    private static KycVerificationHandler CreateHandler(
        out Mock<IKycVerificationProvider> provider,
        out Mock<IKycSessionStore> sessions)
    {
        provider = new Mock<IKycVerificationProvider>();
        sessions = new Mock<IKycSessionStore>();
        sessions
            .Setup(s => s.CreateSessionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("kyc-session-test");
        var opts = Options.Create(new KycOptions { SessionTtlMinutes = 15 });
        return new KycVerificationHandler(provider.Object, sessions.Object, opts);
    }

    [Fact]
    public async Task Verify_PanOnly_Valid_CreatesSession()
    {
        var h = CreateHandler(out var provider, out _);
        provider
            .Setup(p => p.VerifyAsync(It.IsAny<KycProviderInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KycProviderOutcome(true, null, "ref-pan", null));

        var r = await h.VerifyForBookingAsync(Guid.NewGuid(), new VerifyKycRequest("Pan", Pan: "ABCDE1234F"));

        Assert.True(r.Verified);
        Assert.Equal("kyc-session-test", r.SessionId);
        provider.Verify(p => p.VerifyAsync(
            It.Is<KycProviderInput>(i => string.Equals(i.DocumentType, "Pan", StringComparison.Ordinal) && i.NormalizedPan == "ABCDE1234F" && i.NormalizedAadhaarDigits == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Verify_AadhaarOnly_Valid_CreatesSession()
    {
        var h = CreateHandler(out var provider, out _);
        provider
            .Setup(p => p.VerifyAsync(It.IsAny<KycProviderInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KycProviderOutcome(true, null, "ref-uid", null));

        var r = await h.VerifyForBookingAsync(Guid.NewGuid(), new VerifyKycRequest("Aadhaar", Aadhaar: "234523452340"));

        Assert.True(r.Verified);
        provider.Verify(p => p.VerifyAsync(
            It.Is<KycProviderInput>(i => string.Equals(i.DocumentType, "Aadhaar", StringComparison.OrdinalIgnoreCase) && i.NormalizedAadhaarDigits == "234523452340"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Verify_InvalidDocumentType_ReturnsError()
    {
        var h = CreateHandler(out var provider, out _);
        var r = await h.VerifyForBookingAsync(Guid.NewGuid(), new VerifyKycRequest("Passport"));

        Assert.False(r.Verified);
        Assert.Contains("Pan or Aadhaar", r.Message ?? "", StringComparison.OrdinalIgnoreCase);
        provider.Verify(p => p.VerifyAsync(It.IsAny<KycProviderInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Verify_PanPath_InvalidPan_DoesNotCallProvider()
    {
        var h = CreateHandler(out var provider, out _);
        var r = await h.VerifyForBookingAsync(Guid.NewGuid(), new VerifyKycRequest("Pan", Pan: "BAD"));

        Assert.False(r.Verified);
        provider.Verify(p => p.VerifyAsync(It.IsAny<KycProviderInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
