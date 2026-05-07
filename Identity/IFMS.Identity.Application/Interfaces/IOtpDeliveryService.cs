namespace IFMS.Identity.Application.Interfaces;

public interface IOtpDeliveryService
{
    Task SendLoginCodeAsync(string? email, string? phoneDigits, string code);
    Task SendPasswordResetCodeAsync(string? email, string? phoneDigits, string code);
}
