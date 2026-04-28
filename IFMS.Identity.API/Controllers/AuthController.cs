using IFMS.Identity.Application.Commands;
using IFMS.Identity.Application.DTOs;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Application.Support;
using IFMS.Identity.Domain.Constants;
using IFMS.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IFMS.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerHandler;
    private readonly LoginCommandHandler _loginHandler;
    private readonly RequestLoginOtpCommandHandler _requestLoginOtpHandler;
    private readonly VerifyLoginOtpCommandHandler _verifyLoginOtpHandler;
    private readonly RequestPasswordResetOtpCommandHandler _requestResetOtpHandler;
    private readonly ResetPasswordCommandHandler _resetPasswordHandler;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(
        RegisterCommandHandler registerHandler,
        LoginCommandHandler loginHandler,
        RequestLoginOtpCommandHandler requestLoginOtpHandler,
        VerifyLoginOtpCommandHandler verifyLoginOtpHandler,
        RequestPasswordResetOtpCommandHandler requestResetOtpHandler,
        ResetPasswordCommandHandler resetPasswordHandler,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _requestLoginOtpHandler = requestLoginOtpHandler;
        _verifyLoginOtpHandler = verifyLoginOtpHandler;
        _requestResetOtpHandler = requestResetOtpHandler;
        _resetPasswordHandler = resetPasswordHandler;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    // ── Profile ──────────────────────────────────────────────────────────────

    /// <summary>Get the signed-in user's own profile.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var userId))
            return Unauthorized(new { error = "Invalid token" });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        return Ok(new
        {
            user.Id, user.FullName, user.Email,
            user.PhoneNumber, user.Role, user.IsActive, user.CreatedAt
        });
    }

    /// <summary>Update own profile (name and/or phone number).</summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var userId))
            return Unauthorized(new { error = "Invalid token" });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        // Validate phone uniqueness if changing
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var normalized = AuthIdentifier.NormalizePhone(request.PhoneNumber);
            if (string.IsNullOrEmpty(normalized) || normalized.Length < 10)
                return BadRequest(new { error = "Enter a valid 10-digit mobile number." });

            var existing = await _userRepository.GetByPhoneAsync(normalized);
            if (existing != null && existing.Id != userId)
                return Conflict(new { error = "Mobile number already registered to another account." });

            user.UpdateProfile(request.FullName ?? user.FullName, normalized);
        }
        else
        {
            user.UpdateProfile(request.FullName ?? user.FullName, user.PhoneNumber);
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new
        {
            user.Id, user.FullName, user.Email,
            user.PhoneNumber, user.Role, user.IsActive
        });
    }

    /// <summary>Change own password while logged in.</summary>
    [HttpPut("me/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var userId))
            return Unauthorized(new { error = "Invalid token" });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return BadRequest(new { error = "Account uses social login — password change not applicable." });

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { error = "Current password is incorrect." });

        if (request.NewPassword.Length < 8)
            return BadRequest(new { error = "New password must be at least 8 characters." });

        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully." });
    }

    // ── Admin user management ─────────────────────────────────────────────────

    /// <summary>Admin only — list all users, optionally filtered by role.</summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
    {
        var users = await _userRepository.GetAllAsync(role);
        return Ok(users.Select(u => new
        {
            u.Id, u.FullName, u.Email, u.PhoneNumber,
            u.Role, u.IsActive, u.CreatedAt
        }));
    }

    /// <summary>Admin only — get a single user by ID.</summary>
    [HttpGet("users/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new { error = "User not found" });
        return Ok(new
        {
            user.Id, user.FullName, user.Email, user.PhoneNumber,
            user.Role, user.IsActive, user.CreatedAt
        });
    }

    /// <summary>Admin only — change a user's role.</summary>
    [HttpPut("users/{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        if (request.Role != Roles.Admin && request.Role != Roles.Dealer && request.Role != Roles.Customer)
            return BadRequest(new { error = "Invalid role. Must be Admin, Dealer, or Customer." });

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new { error = "User not found" });

        user.ChangeRole(request.Role);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = $"Role updated to {request.Role}.", userId = id, newRole = request.Role });
    }

    /// <summary>Admin only — deactivate a user account.</summary>
    [HttpPut("users/{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new { error = "User not found" });

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "User deactivated.", userId = id });
    }

    /// <summary>Admin only — reactivate a user account.</summary>
    [HttpPut("users/{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new { error = "User not found" });

        user.Activate();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "User activated.", userId = id });
    }

    // ── Auth flows ────────────────────────────────────────────────────────────

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest? request,
        [FromServices] ILogger<AuthController> logger,
        [FromServices] IHostEnvironment hostEnvironment,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is null)
            return BadRequest(new { error = "Request body is required (JSON with fullName, email, password, role)." });

        try
        {
            var result = await _registerHandler.HandleAsync(request);
            return Ok(new { message = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            if (IsSqlUniqueConstraintViolation(ex))
            {
                logger.LogWarning(ex, "Register rejected: duplicate email or phone.");
                return Conflict(new { error = "This email or mobile number is already registered." });
            }
            logger.LogError(ex, "Register failed.");
            var root = ex.GetBaseException();
            string error;
            if (root is SqlException)
                error = "Cannot reach SQL Server or login failed.";
            else if (ex is DbUpdateException)
                error = "Could not save the account. Apply Identity EF migrations.";
            else
                error = "Registration failed. Check the Identity API console log for the exception.";

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error,
                detail = hostEnvironment.IsDevelopment() ? root.Message : null
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _loginHandler.HandleAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("login/otp/request")]
    public async Task<IActionResult> RequestLoginOtp([FromBody] RequestOtpRequest request)
    {
        try
        {
            var minutes = _configuration.GetValue("Auth:OtpExpiryMinutes", 10);
            var devCode = await _requestLoginOtpHandler.HandleAsync(request, minutes);
            var response = new Dictionary<string, object>
            {
                ["message"] = "If an account exists for that email or mobile number, a code has been sent."
            };
            // In development or when SMS/Email is not configured, return the code directly
            // so it can be used without a real SMS provider.
            if (!string.IsNullOrEmpty(devCode))
                response["devCode"] = devCode;
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login/otp/verify")]
    public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyOtpLoginRequest request)
    {
        try
        {
            var result = await _verifyLoginOtpHandler.HandleAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("password/reset/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetOtpRequest request)
    {
        try
        {
            var minutes = _configuration.GetValue("Auth:OtpExpiryMinutes", 10);
            var devCode = await _requestResetOtpHandler.HandleAsync(request, minutes);
            var response = new Dictionary<string, object>
            {
                ["message"] = "If an account exists for that email or mobile number, a reset code has been sent."
            };
            if (!string.IsNullOrEmpty(devCode))
                response["devCode"] = devCode;
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("password/reset/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _resetPasswordHandler.HandleAsync(request);
            return Ok(new { message = "Password updated. You can sign in with your new password." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static bool IsSqlUniqueConstraintViolation(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException)
            if (e is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
                return true;
        return false;
    }
}
