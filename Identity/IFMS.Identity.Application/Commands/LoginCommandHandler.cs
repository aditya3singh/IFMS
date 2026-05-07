using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Commands;

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> HandleAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null)
            throw new InvalidOperationException("Invalid email or password.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new InvalidOperationException("This account has no password. Use OTP sign-in or Forgot password to set one.");

        bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValid)
            throw new InvalidOperationException("Invalid email or password.");

        string token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(token, user.FullName, user.Email, user.Role);
    }
}