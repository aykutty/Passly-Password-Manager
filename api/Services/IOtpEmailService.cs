using Passly.Enums;

namespace Passly.Services;

public interface IOtpEmailService
{
    Task SendEmailOtpAsync(
        string toEmail,
        string otpCode,
        OtpPurpose otpPurpose,
        int? expiryMinutes = null, 
        CancellationToken ct = default);
}