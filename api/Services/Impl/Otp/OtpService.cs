using Microsoft.Extensions.Options;
using Passly.Entities;
using Passly.Enums;
using Passly.Options;
using Passly.Repositories;

namespace Passly.Services.Impl.Otp;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly ILogger<OtpService> _logger;
    private readonly OtpOptions _otpOptions;
    private readonly IOtpEmailService _otpEmailService;
    private readonly IUserRepository _userRepository;

    public OtpService(IOtpRepository otpRepository,
        IOptions<OtpOptions> otpOptions,
        IOtpEmailService otpEmailService, ILogger<OtpService> logger, IUserRepository userRepository)
    {
        _otpRepository = otpRepository;
        _logger = logger;
        _otpOptions = otpOptions.Value;
        _otpEmailService = otpEmailService;
        _userRepository = userRepository;
    }

    public async Task<string> GenerateOtpAsync(string email, OtpPurpose otpPurpose, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        
        email = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetUserByEmailAsync(email, ct);
        
        if (user is null && otpPurpose != OtpPurpose.EmailVerification)
        {
            _logger.LogWarning("OTP generation requested for non-existent email {Email}", email);
            throw new InvalidOperationException("No user found for the specified email.");
        }
        
        var otpCode = GenerateOtpCode(_otpOptions.Length);
        var otp = new Entities.Otp
        {
            UserId = user?.Id,
            Email = email,
            Expiration = DateTime.UtcNow.AddMinutes(_otpOptions.ExpiryMinutes),
            OtpCode = otpCode,
            Purpose = otpPurpose,
            AttemptCount = 0,
            IsUsed = false
        };

        await _otpRepository.AddAsync(otp,ct);
        _logger.LogInformation("Generated OTP for user {UserId} ({Purpose})", user.Id, otpPurpose);

        await _otpEmailService.SendEmailOtpAsync(email, otpCode, otpPurpose, _otpOptions.ExpiryMinutes, ct);
        return otpCode;
    }

    public async Task<bool> VerifyOtpAsync(string email, string inputOtp, OtpPurpose otpPurpose, CancellationToken ct = default)
    {
        var otp = await _otpRepository.GetLatestValidOtpAsync(email, otpPurpose, ct);

        if (otp is null)
        {
            _logger.LogWarning("No valid OTP found for email {Email}", email);
            return false;
        }
        
        if (otp.Expiration <= DateTime.UtcNow)
        {
            _logger.LogInformation("OTP expired for email {Email}", email);
            otp.IsUsed = true;
            await _otpRepository.UpdateAsync(otp, ct);
            return false;
        }
        
        if (otp.AttemptCount >= _otpOptions.MaxAttempts)
        {
            _logger.LogWarning("Email {Email} exceeded max OTP attempts.", email);
            otp.IsUsed = true;
            await _otpRepository.UpdateAsync(otp, ct);
            return false;
        }
        
        if (otp.OtpCode != inputOtp)
        {
            otp.AttemptCount++;
            await _otpRepository.UpdateAsync(otp, ct);
            _logger.LogWarning("Invalid OTP entered for email {Email}. Attempt {AttemptCount}/{MaxAttempts}",
                email, otp.AttemptCount, _otpOptions.MaxAttempts);
            return false;
        }
        
        otp.IsUsed = true;
        await _otpRepository.UpdateAsync(otp, ct);
        _logger.LogInformation("OTP successfully verified for email {Email}", email);
        return true;
    }

    public async Task CleanExpiredOtpsAsync()
    {
        var expiredOtps = await _otpRepository.GetExpiredOtpsAsync();
        
        if (expiredOtps.Any())
        {
            await _otpRepository.RemoveRangeAsync(expiredOtps);
            _logger.LogInformation("Cleaned {Count} expired OTPs", expiredOtps.Count);
        }
    }
    
    private static string GenerateOtpCode(int lenght)
    {
        var r = new Random();
        return string.Concat(Enumerable.Range(0, lenght).Select(_ => r.Next(0, 10)));
    }
}