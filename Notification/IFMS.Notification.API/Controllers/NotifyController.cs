using IFMS.Notification.API.DTOs;
using IFMS.Notification.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IFMS.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Internal service-to-service — secured by X-Internal-Key header
public class NotifyController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotifyController> _logger;

    public NotifyController(
        INotificationService notificationService,
        IConfiguration configuration,
        ILogger<NotifyController> logger)
    {
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("token")]
    public async Task<IActionResult> SendTokenNotification(
        [FromHeader(Name = "X-Internal-Key")] string? apiKey,
        [FromBody] SendTokenNotificationRequest request)
    {
        if (!IsAuthorized(apiKey)) return Unauthorized();

        _logger.LogInformation("Sending booking token notification to {Email}", request.CustomerEmail);

        var smsMessage = $"Your IFMS booking token: {request.TokenCode}. Valid 24 hours. " +
                         $"Fuel: {request.FuelType}, Qty: {request.Quantity}L at {request.StationName}.";

        var emailBody = $@"
            <h2>IFMS Booking Confirmation</h2>
            <p>Dear {request.CustomerName},</p>
            <p>Your fuel booking has been confirmed!</p>
            <table style='border-collapse:collapse;'>
                <tr><td style='padding:8px;border:1px solid #ddd;'><b>Token Code</b></td>
                    <td style='padding:8px;border:1px solid #ddd;'><b>{request.TokenCode}</b></td></tr>
                <tr><td style='padding:8px;border:1px solid #ddd;'>Station</td>
                    <td style='padding:8px;border:1px solid #ddd;'>{request.StationName}</td></tr>
                <tr><td style='padding:8px;border:1px solid #ddd;'>Fuel Type</td>
                    <td style='padding:8px;border:1px solid #ddd;'>{request.FuelType}</td></tr>
                <tr><td style='padding:8px;border:1px solid #ddd;'>Quantity</td>
                    <td style='padding:8px;border:1px solid #ddd;'>{request.Quantity} Litres</td></tr>
                <tr><td style='padding:8px;border:1px solid #ddd;'>Amount Paid</td>
                    <td style='padding:8px;border:1px solid #ddd;'>₹{request.TotalPaid}</td></tr>
            </table>
            <p>This token is valid for <b>24 hours</b>. Show this token at the pump to collect your fuel.</p>
            <p>Thank you for using IFMS!</p>";

        await _notificationService.SendSmsAsync(request.CustomerPhone, smsMessage);
        await _notificationService.SendEmailAsync(request.CustomerEmail, "IFMS Booking Confirmation", emailBody);

        return Ok(new { message = "Token notification sent successfully" });
    }

    [HttpPost("low-stock")]
    public async Task<IActionResult> SendLowStockAlert(
        [FromHeader(Name = "X-Internal-Key")] string? apiKey,
        [FromBody] SendLowStockAlertRequest request)
    {
        if (!IsAuthorized(apiKey)) return Unauthorized();

        _logger.LogInformation("Sending low stock alert for station {Station}", request.StationName);

        var smsMessage = $"IFMS Alert: {request.FuelType} stock low at {request.StationName}. " +
                         $"Remaining: {request.RemainingQuantity}L. Please restock immediately.";

        var emailBody = $@"
            <h2>⚠️ Low Stock Alert</h2>
            <p>Station <b>{request.StationName}</b> has low fuel stock.</p>
            <p>Fuel Type: <b>{request.FuelType}</b></p>
            <p>Remaining: <b>{request.RemainingQuantity} Litres</b></p>
            <p>Please arrange restocking immediately.</p>";

        await _notificationService.SendSmsAsync(request.DealerPhone, smsMessage);
        await _notificationService.SendEmailAsync(request.DealerEmail, "IFMS Low Stock Alert", emailBody);

        return Ok(new { message = "Low stock alert sent successfully" });
    }

    [HttpPost("fraud-alert")]
    public async Task<IActionResult> SendFraudAlert(
        [FromHeader(Name = "X-Internal-Key")] string? apiKey,
        [FromBody] SendFraudAlertRequest request)
    {
        if (!IsAuthorized(apiKey)) return Unauthorized();

        _logger.LogInformation("Sending fraud alert for station {Station}", request.StationName);

        var smsMessage = $"IFMS FRAUD ALERT: Suspicious activity at {request.StationName}. " +
                         $"Amount: ₹{request.SuspiciousAmount}. Check admin dashboard.";

        var emailBody = $@"
            <h2>🚨 Fraud Alert</h2>
            <p>Suspicious transaction detected at <b>{request.StationName}</b>.</p>
            <p>Description: {request.Description}</p>
            <p>Amount: <b>₹{request.SuspiciousAmount}</b></p>
            <p>Date: {request.TransactionDate:yyyy-MM-dd HH:mm}</p>
            <p>Please investigate immediately via the Admin Dashboard.</p>";

        await _notificationService.SendSmsAsync(request.AdminPhone, smsMessage);
        await _notificationService.SendEmailAsync(request.AdminEmail, "IFMS Fraud Alert", emailBody);

        return Ok(new { message = "Fraud alert sent successfully" });
    }

    private bool IsAuthorized(string? apiKey)
    {
        var expected = _configuration["InternalApiKey"];
        return !string.IsNullOrEmpty(expected) && apiKey == expected;
    }
}
