using Passly.Entities;
using Passly.Enums;

namespace Passly.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email, OtpPurpose otpPurpose, CancellationToken ct = default);
    Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose otpPurpose, CancellationToken ct = default);
    Task CleanExpiredOtpsAsync();
    
}