using IFMS.Identity.Application.Commands;
using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Domain.Entities;
using Moq;

namespace IFMS.Identity.Tests;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ISelfRegistrationPolicy> _policyMock = new();

    public RegisterCommandHandlerTests()
    {
        _policyMock.Setup(p => p.EnsureAllowedForPublicSignup(It.IsAny<string>()));
    }

    [Fact]
    public async Task Register_ValidInput_ReturnsSuccess()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        var handler = new RegisterCommandHandler(_userRepoMock.Object, _policyMock.Object);

        var result = await handler.HandleAsync(new RegisterRequest("Test User", "test@ifms.com", "Pass@12345", "Customer"));

        Assert.Equal("User registered successfully.", result);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsException()
    {
        _userRepoMock.Setup(r => r.ExistsAsync("dup@ifms.com")).ReturnsAsync(true);
        var handler = new RegisterCommandHandler(_userRepoMock.Object, _policyMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new RegisterRequest("Dup User", "dup@ifms.com", "Pass@12345", "Customer")));
    }

    [Fact]
    public async Task Register_InvalidRole_ThrowsException()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        var handler = new RegisterCommandHandler(_userRepoMock.Object, _policyMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new RegisterRequest("Bad Role", "bad@ifms.com", "Pass@12345", "SuperAdmin")));
    }

    [Fact]
    public async Task Register_ShortPassword_ThrowsException()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        var handler = new RegisterCommandHandler(_userRepoMock.Object, _policyMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new RegisterRequest("Short", "s@ifms.com", "short", "Customer")));
    }
}

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123");
        var user = User.Create("Test", "test@ifms.com", passwordHash, "Customer");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@ifms.com")).ReturnsAsync(user);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("mock-jwt-token");

        var handler = new LoginCommandHandler(_userRepoMock.Object, _jwtMock.Object);
        var result = await handler.HandleAsync(new LoginRequest("test@ifms.com", "Pass@123"));

        Assert.Equal("mock-jwt-token", result.Token);
        Assert.Equal("Test", result.FullName);
        Assert.Equal("Customer", result.Role);
    }

    [Fact]
    public async Task Login_WrongEmail_ThrowsException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var handler = new LoginCommandHandler(_userRepoMock.Object, _jwtMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new LoginRequest("no@ifms.com", "Pass@123")));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsException()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = User.Create("Test", "test@ifms.com", passwordHash, "Customer");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@ifms.com")).ReturnsAsync(user);

        var handler = new LoginCommandHandler(_userRepoMock.Object, _jwtMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new LoginRequest("test@ifms.com", "WrongPassword")));
    }
}

public class UserEntityTests
{
    [Fact]
    public void Create_ValidInput_SetsAllFields()
    {
        var user = User.Create("John Doe", "john@ifms.com", "hash123", "Admin");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("John Doe", user.FullName);
        Assert.Equal("john@ifms.com", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.Equal("Admin", user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = User.Create("Test", "t@t.com", "hash", "Customer");
        user.Deactivate();
        Assert.False(user.IsActive);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var user = User.Create("Test", "t@t.com", "hash", "Customer");
        user.Deactivate();
        user.Activate();
        Assert.True(user.IsActive);
    }
}
