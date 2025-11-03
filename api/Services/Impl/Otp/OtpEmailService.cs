using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Passly.Enums;
using Passly.Options;

namespace Passly.Services.Impl.Otp;

public class OtpEmailService : IOtpEmailService
{
    private readonly ILogger<OtpEmailService> _logger;
    private readonly SmtpOptions _smtpOptions;
    private readonly SmtpClient _smtpClient;

    public OtpEmailService(ILogger<OtpEmailService> logger, SmtpClient smtpClient, IOptions<SmtpOptions> smtpOptions)
    {
        _logger = logger;
        _smtpOptions = smtpOptions.Value;
        _smtpClient = smtpClient;
    }

    public async Task SendEmailOtpAsync(string toEmail, string otpCode, OtpPurpose otpPurpose, int? expiryMinutes,
        CancellationToken ct = default)
    {
        using var mail = new MailMessage
        {
            From = new MailAddress(_smtpOptions.FromAddress, _smtpOptions.DisplayName),
            Subject = otpPurpose switch
            {
                OtpPurpose.EmailVerification => "Verify your email address",
                OtpPurpose.LoginVerification => "Your login verification code",
                OtpPurpose.PasswordReset => "Password reset code",
                _ => "Your verification code"
            },
            Body = $"Your one-time code is {otpCode}. It will expire in {expiryMinutes ?? 5} minutes.",
            IsBodyHtml = false
        };

        mail.To.Add(toEmail);
        
        await _smtpClient.SendMailAsync(mail,ct);
        _logger.LogInformation("OTP email sent to {Email} for purpose {Purpose}", toEmail, otpPurpose);
    }
}