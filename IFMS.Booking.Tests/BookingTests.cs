using IFMS.Booking.Application.Commands;
using IFMS.Booking.Application.DTOs;
using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Application.Options;
using Microsoft.Extensions.Options;
using Moq;
using BookingEntity = IFMS.Booking.Domain.Entities.Booking;

namespace IFMS.Booking.Tests;

public class BookingCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _repoMock = new();
    private readonly Mock<ITokenCacheService> _cacheMock = new();
    private readonly Mock<IKycSessionStore> _kycSessionsMock = new();

    private BookingCommandHandler CreateHandler(bool requireKyc = false)
    {
        _kycSessionsMock
            .Setup(s => s.TryConsumeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var flow = Options.Create(new BookingFlowOptions { RequireKycVerification = requireKyc });
        return new BookingCommandHandler(_repoMock.Object, _cacheMock.Object, _kycSessionsMock.Object, flow);
    }

    [Fact]
    public async Task CreateBooking_ValidInput_ReturnsResponseWithToken()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<BookingEntity>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _cacheMock.Setup(c => c.StoreTokenAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var customerId = Guid.NewGuid();
        var result = await handler.CreateBookingAsync(
            new CreateBookingRequest(customerId, Guid.NewGuid(), 5, "Petrol", 10, 105.50m, "PAY-123"),
            customerId);

        Assert.NotNull(result.TokenCode);
        Assert.StartsWith("IFM-", result.TokenCode);
        Assert.Equal(15, result.TokenCode.Length);
        Assert.Equal("PENDING", result.TokenStatus);
        Assert.Equal(1055.00m, result.TotalPaid);
    }

    [Fact]
    public async Task CreateBooking_RequireKyc_MissingSession_Throws()
    {
        var handler = CreateHandler(requireKyc: true);
        var customerId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.CreateBookingAsync(
                new CreateBookingRequest(customerId, Guid.NewGuid(), 5, "Petrol", 10, 105.50m, "PAY-1", null),
                customerId));
    }

    [Fact]
    public async Task CreateBooking_CustomerMismatch_Throws()
    {
        var handler = CreateHandler();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.CreateBookingAsync(
                new CreateBookingRequest(Guid.NewGuid(), Guid.NewGuid(), 5, "Petrol", 10, 105.50m, "PAY-2"),
                Guid.NewGuid()));
    }

    [Fact]
    public async Task ValidateToken_CachedToken_ReturnsDetails()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Diesel", 20, 1900, "PAY-456", 3);
        _cacheMock.Setup(c => c.GetTokenAsync(booking.TokenCode)).ReturnsAsync("cached");
        _repoMock.Setup(r => r.GetByTokenCodeAsync(booking.TokenCode)).ReturnsAsync(booking);

        var handler = CreateHandler();
        var result = await handler.ValidateTokenAsync(booking.TokenCode);

        Assert.Equal(booking.TokenCode, result.TokenCode);
        Assert.Equal("Diesel", result.FuelType);
        Assert.Equal(20, result.QuantityLiters);
    }

    [Fact]
    public async Task ValidateToken_NotInCache_ThrowsException()
    {
        _cacheMock.Setup(c => c.GetTokenAsync(It.IsAny<string>())).ReturnsAsync((string?)null);

        var handler = CreateHandler();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.ValidateTokenAsync("IFM-01-99999999"));
    }

    [Fact]
    public async Task ConfirmBooking_PendingToken_MarksUsed()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Petrol", 10, 1050, "PAY-789", 1);
        _repoMock.Setup(r => r.GetByTokenCodeAsync(booking.TokenCode)).ReturnsAsync(booking);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _cacheMock.Setup(c => c.DeleteTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var result = await handler.ConfirmBookingAsync(booking.TokenCode);

        Assert.Equal("USED", result.TokenStatus);
        Assert.NotNull(result.UsedAt);
    }

    [Fact]
    public async Task ConfirmBooking_AlreadyUsed_ThrowsException()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Petrol", 10, 1050, "PAY-789", 1);
        booking.MarkUsed();
        _repoMock.Setup(r => r.GetByTokenCodeAsync(booking.TokenCode)).ReturnsAsync(booking);

        var handler = CreateHandler();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.ConfirmBookingAsync(booking.TokenCode));
    }

    [Fact]
    public async Task CancelBooking_PendingToken_MarksCancelled()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Diesel", 15, 1425, "PAY-000", 2);
        _repoMock.Setup(r => r.GetByTokenCodeAsync(booking.TokenCode)).ReturnsAsync(booking);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _cacheMock.Setup(c => c.DeleteTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var result = await handler.CancelBookingAsync(booking.TokenCode);

        Assert.Equal("CANCELLED", result.TokenStatus);
    }

    [Fact]
    public async Task CancelBooking_AlreadyUsed_ThrowsException()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Petrol", 10, 1050, "PAY-999", 1);
        booking.MarkUsed();
        _repoMock.Setup(r => r.GetByTokenCodeAsync(booking.TokenCode)).ReturnsAsync(booking);

        var handler = CreateHandler();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.CancelBookingAsync(booking.TokenCode));
    }
}

public class BookingEntityTests
{
    [Fact]
    public void Create_GeneratesValidToken()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Petrol", 10, 1050, "PAY-1", 5);

        Assert.StartsWith("IFM-05-", booking.TokenCode);
        Assert.Equal(15, booking.TokenCode.Length);
        Assert.Equal("PENDING", booking.TokenStatus);
        Assert.Null(booking.UsedAt);
    }

    [Fact]
    public void GenerateToken_FormatIsCorrect()
    {
        var token = BookingEntity.GenerateToken(7);
        Assert.Matches(@"^IFM-07-\d{8}$", token);
    }

    [Fact]
    public void GenerateToken_IsRandom()
    {
        var tokens = Enumerable.Range(0, 100).Select(_ => BookingEntity.GenerateToken(1)).ToHashSet();
        Assert.True(tokens.Count > 90, "Tokens should be mostly unique");
    }

    [Fact]
    public void MarkUsed_SetsStatusAndTimestamp()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Diesel", 20, 1900, "PAY-2", 3);
        booking.MarkUsed();
        Assert.Equal("USED", booking.TokenStatus);
        Assert.NotNull(booking.UsedAt);
    }

    [Fact]
    public void MarkExpired_SetsStatus()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "CNG", 5, 400, "PAY-3", 1);
        booking.MarkExpired();
        Assert.Equal("EXPIRED", booking.TokenStatus);
    }

    [Fact]
    public void MarkCancelled_SetsStatus()
    {
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Petrol", 10, 1050, "PAY-4", 2);
        booking.MarkCancelled();
        Assert.Equal("CANCELLED", booking.TokenStatus);
    }

    [Fact]
    public void Create_SetsExpiresAt24Hours()
    {
        var before = DateTime.UtcNow.AddHours(24);
        var booking = BookingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "Petrol", 10, 1050, "PAY-5", 1);
        var after = DateTime.UtcNow.AddHours(24);

        Assert.InRange(booking.ExpiresAt, before.AddSeconds(-1), after.AddSeconds(1));
    }
}
