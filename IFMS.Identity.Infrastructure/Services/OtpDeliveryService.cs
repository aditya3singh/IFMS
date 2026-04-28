using System.Net.Http.Json;
using IFMS.Identity.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace IFMS.Identity.Infrastructure.Services;

public class OtpDeliveryService : IOtpDeliveryService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<OtpDeliveryService> _logger;

    public OtpDeliveryService(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<OtpDeliveryService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public Task SendLoginCodeAsync(string? email, string? phoneDigits, string code)
        => SendAsync(email, phoneDigits, code, "login");

    public Task SendPasswordResetCodeAsync(string? email, string? phoneDigits, string code)
        => SendAsync(email, phoneDigits, code, "password-reset");

    private async Task SendAsync(string? email, string? phoneDigits, string code, string kind)
    {
        _logger.LogWarning(
            "[IFMS OTP {Kind}] Email={Email} Phone={Phone} Code={Code}",
            kind,
            email ?? "(none)",
            phoneDigits ?? "(none)",
            code);

        var baseUrl = _config["Auth:NotificationBaseUrl"]?.TrimEnd('/');
        var apiKey = _config["Auth:InternalNotifyKey"];
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
            return;

        try
        {
            var client = _httpFactory.CreateClient("ifms-notify");
            client.DefaultRequestHeaders.Remove("X-Internal-Key");
            client.DefaultRequestHeaders.Add("X-Internal-Key", apiKey);

            var resp = await client.PostAsJsonAsync($"{baseUrl}/api/internal/otp", new
            {
                email,
                phoneDigits,
                code,
                kind
            });
            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Notification service OTP call failed: {Status}", (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notification service OTP call error");
        }
    }
}
