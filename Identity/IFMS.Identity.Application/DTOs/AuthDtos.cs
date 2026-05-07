namespace IFMS.Identity.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? PhoneNumber = null
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string FullName,
    string Email,
    string Role
);

public record RequestOtpRequest(string Identifier);

public record VerifyOtpLoginRequest(string Identifier, string Code);

public record RequestPasswordResetOtpRequest(string Identifier);

public record ResetPasswordRequest(string Identifier, string Code, string NewPassword);

public record UpdateProfileRequest(string? FullName, string? PhoneNumber);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ChangeRoleRequest(string Role);
public record AdminSetPasswordRequest(string NewPassword);
