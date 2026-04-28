using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Application.Support;
using IFMS.Identity.Domain.Constants;
using IFMS.Identity.Domain.Entities;

namespace IFMS.Identity.Application.Commands;

public class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ISelfRegistrationPolicy _selfRegistration;

    public RegisterCommandHandler(IUserRepository userRepository, ISelfRegistrationPolicy selfRegistration)
    {
        _userRepository = userRepository;
        _selfRegistration = selfRegistration;
    }

    public async Task<string> HandleAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new InvalidOperationException("Full name is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Password is required.");
        if (string.IsNullOrWhiteSpace(request.Role))
            throw new InvalidOperationException("Role is required.");

        var emailNorm = AuthIdentifier.NormalizeEmail(request.Email);
        if (await _userRepository.ExistsAsync(emailNorm))
            throw new InvalidOperationException("Email already registered.");

        string? phoneNorm = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phoneNorm = AuthIdentifier.NormalizePhone(request.PhoneNumber);
            if (string.IsNullOrEmpty(phoneNorm) || phoneNorm.Length < 10)
                throw new InvalidOperationException("Enter a valid 10-digit mobile number.");
            if (await _userRepository.ExistsPhoneAsync(phoneNorm))
                throw new InvalidOperationException("Mobile number already registered.");
        }

        if (request.Role != Roles.Admin &&
            request.Role != Roles.Dealer &&
            request.Role != Roles.Customer)
            throw new InvalidOperationException("Invalid role.");

        _selfRegistration.EnsureAllowedForPublicSignup(request.Role);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.FullName, emailNorm, passwordHash, request.Role, phoneNorm);
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return "User registered successfully.";
    }
}