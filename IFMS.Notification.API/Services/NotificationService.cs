using System.Net;
using System.Net.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace IFMS.Notification.API.Services;

public interface INotificationService
{
    Task SendSmsAsync(string toPhone, string message);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}

/// <summary>
/// Real implementation — Twilio for SMS, Gmail SMTP for email.
/// </summary>
public class RealNotificationService : INotificationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<RealNotificationService> _logger;

    public RealNotificationService(IConfiguration config, ILogger<RealNotificationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendSmsAsync(string toPhone, string message)
    {
        var sid   = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];
        var from  = _config["Twilio:FromPhone"];

        if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(from))
        {
            _logger.LogWarning("[SMS] Twilio not configured — skipping SMS to {Phone}", toPhone);
            return;
        }

        try
        {
            TwilioClient.Init(sid, token);
            var msg = await MessageResource.CreateAsync(
                to:   new PhoneNumber(toPhone),
                from: new PhoneNumber(from),
                body: message);
            _logger.LogInformation("[SMS] Sent to {Phone} — SID: {Sid}", toPhone, msg.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SMS] Failed to send to {Phone}", toPhone);
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var gmailUser = _config["Gmail:User"];
        var gmailPass = _config["Gmail:AppPassword"];

        if (string.IsNullOrEmpty(gmailUser) || string.IsNullOrEmpty(gmailPass))
        {
            _logger.LogWarning("[EMAIL] Gmail not configured — skipping email to {Email}", toEmail);
            return;
        }

        try
        {
            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl   = true,
                Credentials = new NetworkCredential(gmailUser, gmailPass)
            };

            using var mail = new MailMessage
            {
                From       = new MailAddress(gmailUser, "IFMS — Bharat Kinetic"),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
            _logger.LogInformation("[EMAIL] Sent to {Email} — Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send to {Email}", toEmail);
        }
    }
}
