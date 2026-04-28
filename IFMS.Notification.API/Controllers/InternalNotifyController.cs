using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IFMS.Notification.API.Controllers;

[ApiController]
[Route("api/internal")]
public class InternalNotifyController : ControllerBase
{
    private readonly INotificationService _notify;
    private readonly NotificationStore _store;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InternalNotifyController> _logger;

    public InternalNotifyController(
        INotificationService notify,
        NotificationStore store,
        IConfiguration configuration,
        ILogger<InternalNotifyController> logger)
    {
        _notify = notify;
        _store = store;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Service-to-service OTP delivery. Secured with X-Internal-Key.</summary>
    [HttpPost("otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp(
        [FromHeader(Name = "X-Internal-Key")] string? apiKey,
        [FromBody] OtpInternalRequest request)
    {
        var expected = _configuration["InternalApiKey"];
        if (string.IsNullOrEmpty(expected) || apiKey != expected)
            return Unauthorized();

        var subject = request.Kind.Contains("reset", StringComparison.OrdinalIgnoreCase)
            ? "IFMS password reset code"
            : "IFMS sign-in code";

        var body = $@"<p>Your one-time code is:</p>
            <p style='font-size:24px;font-weight:bold;letter-spacing:4px;'>{request.Code}</p>
            <p>This code expires in a few minutes. If you did not request it, ignore this email.</p>";

        if (!string.IsNullOrWhiteSpace(request.Email))
            await _notify.SendEmailAsync(request.Email.Trim(), subject, body);

        if (!string.IsNullOrWhiteSpace(request.PhoneDigits))
        {
            var sms = $"IFMS code: {request.Code}. Do not share.";
            var phone = request.PhoneDigits!.Trim();
            if (!phone.StartsWith('+')) phone = "+91" + phone;
            await _notify.SendSmsAsync(phone, sms);
        }

        _logger.LogInformation("Internal OTP dispatched for kind {Kind}", request.Kind);
        return Ok(new { message = "sent" });
    }

    /// <summary>Push an in-app notification from any service. Secured with X-Internal-Key.</summary>
    [HttpPost("push")]
    [AllowAnonymous]
    public IActionResult PushNotification(
        [FromHeader(Name = "X-Internal-Key")] string? apiKey,
        [FromBody] CreateNotificationRequest request)
    {
        var expected = _configuration["InternalApiKey"];
        if (string.IsNullOrEmpty(expected) || apiKey != expected)
            return Unauthorized();

        var notification = new AppNotification
        {
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Icon = request.Icon,
            TargetRole = request.TargetRole,
            TargetUserId = request.TargetUserId
        };
        _store.Add(notification);
        _logger.LogInformation("Push notification: {Title} → {Role}", request.Title, request.TargetRole);
        return Ok(notification);
    }

    /// <summary>
    /// Send a plain SMS to a customer's registered mobile number.
    /// Used by Booking API for fuel-available, fuel-unavailable, and fuel-dispensed events.
    /// Secured with X-Internal-Key.
    /// </summary>
    [HttpPost("sms")]
    [AllowAnonymous]
    public async Task<IActionResult> SendSms(
        [FromHeader(Name = "X-Internal-Key")] string? apiKey,
        [FromBody] SmsInternalRequest request)
    {
        var expected = _configuration["InternalApiKey"];
        if (string.IsNullOrEmpty(expected) || apiKey != expected)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.ToPhone))
            return BadRequest(new { error = "toPhone is required" });

        var phone = request.ToPhone.Trim();
        if (!phone.StartsWith('+')) phone = "+91" + phone;

        await _notify.SendSmsAsync(phone, request.Message);
        _logger.LogInformation("Internal SMS dispatched to {Phone}", phone);
        return Ok(new { message = "sent" });
    }
}
