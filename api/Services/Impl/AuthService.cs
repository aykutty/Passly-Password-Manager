using Microsoft.Extensions.Options;
using Passly.DTOs.Response;
using Passly.Entities;
using Passly.Enums;
using Passly.Options;
using Passly.Repositories;

namespace Passly.Services.Impl;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IOtpService _otpService;
    private readonly IRefreshTokenRepository _refreshTokenRepository; 
    private readonly ILogger<AuthService> _logger;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordHashingOptions _hashingOptions;

    public AuthService(IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokens,
        IOtpService otpService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<AuthService> logger,
        IOptions<JwtOptions> jwtOptions,
        IOptions<PasswordHashingOptions> hashingOptions)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokens;
        _otpService = otpService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
        _hashingOptions = hashingOptions.Value;
    }
    public async Task RegisterAsync(string email, string password, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested(); 
        email = NormalizeEmail(email);
        
        if (await _userRepository.IsExistsByEmailAsync(email, ct))
        {
            _logger.LogInformation("Registration attempt for existing email {Email}", email);
            throw new InvalidOperationException("Email is already registered.");
        }
        
        var (hash, salt) = await _passwordService.HashPasswordAsync(password, ct);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Iterations = _hashingOptions.Iterations,
            MemorySize = _hashingOptions.MemorySize,
            DegreeOfParallelism = _hashingOptions.DegreeOfParallelism,
            HashSize = _hashingOptions.HashSize
        };

        await _userRepository.AddUserAsync(user);
        _logger.LogInformation("User {UserId} registered with email {Email}", user.Id, email);
        
        await _otpService.GenerateOtpAsync(email, OtpPurpose.EmailVerification, ct);

    }

    public async Task<AuthResponse> LoginWithPasswordAsync(string email, string password, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested(); 
        var user = await _userRepository.GetUserByEmailAsync(email, ct);
        
        if (user is null)
        {
            await _passwordService.PerformDummyHashAsync(email, password, ct);
            _logger.LogWarning("Login failed for non-existent email {Email}", email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        
        var ok = await _passwordService.VerifyPasswordAsync(user, password, ct);
        
        if (!ok)
        {
            _logger.LogWarning("Invalid password for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return IssueTokensAsync(user);

    }

    public async Task RequestEmailVerificationAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await RequireUser(userId, ct);
        await _otpService.GenerateOtpAsync(user.Email, OtpPurpose.EmailVerification, ct);

    }

    public async Task<bool> VerifyEmailAsync(Guid userId, string otp, CancellationToken ct = default)
    {
        var user = await RequireUser(userId, ct);

        var isVerified = await _otpService.VerifyOtpAsync(user.Email,otp, OtpPurpose.EmailVerification,ct);

        if (!isVerified)
        {
            _logger.LogWarning("Email verification OTP failed for user {UserId}", user.Id);
            return false;
        }
        
        if (!user.EmailVerified)
        {
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, ct);
            _logger.LogInformation("User {UserId} email marked as verified", user.Id);
        }

        return true;
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        email = NormalizeEmail(email);
        var user = await _userRepository.GetUserByEmailAsync(email, ct);
        
        if (user is null)
        {
            _logger.LogInformation("Password reset requested for non-existent email {Email}", email);
            return;
        }
        
        await _otpService.GenerateOtpAsync(email, OtpPurpose.PasswordReset, ct);
    }

    public async Task ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        email = NormalizeEmail(email);

        var user = await _userRepository.GetUserByEmailAsync(email, ct);
        
        if (user is null)
        {
            await  _passwordService.PerformDummyHashAsync(email, newPassword, ct);
            _logger.LogWarning("Password reset for non-existent email {Email}", email);
            throw new UnauthorizedAccessException("Invalid reset token or email.");
        }
        
        var ok = await _otpService.VerifyOtpAsync(email, otp, OtpPurpose.PasswordReset, ct);
        if (!ok)
        {
            _logger.LogWarning("Invalid password reset OTP for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid reset token or email.");
        }
        
        var (hash, salt) = await _passwordService.HashPasswordAsync(newPassword, ct);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        var activeToken = await _refreshTokenRepository.GetActiveRefreshTokenAsync(user.Id, ct);
        if (activeToken is not null)
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(activeToken, ct);
        }
        
        _logger.LogInformation("Password reset completed for user {UserId}", user.Id);

    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var valid = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken, ct);
        
        if (valid is null)
        {
            _logger.LogWarning("Invalid or expired refresh token during refresh attempt.");
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }
        
        var user = await _userRepository.GetUserByIdAsync(valid.UserId, ct)
                   ?? throw new UnauthorizedAccessException("Account no longer exists.");
        
        var newPlainRefresh = await _refreshTokenService.RotateRefreshTokenAsync(valid, ct);
        var accessToken = _jwtService.GenerateAccessToken(user);

        _logger.LogInformation("Rotated refresh token and issued new access token for user {UserId}", user.Id);
        
        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes)
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var validToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken, ct);
        if (validToken is null)
        {
            _logger.LogInformation("Logout called with invalid/expired token");
            return;
        }
        await _refreshTokenService.RevokeRefreshTokenAsync(validToken, ct);
        _logger.LogInformation("Refresh token revoked for user {UserId}", validToken.UserId);
    }
    
    private static string NormalizeEmail(string email)
        => email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));

    private async Task<User> RequireUser(Guid userId, CancellationToken ct)
        => await _userRepository.GetUserByIdAsync(userId, ct)
           ?? throw new InvalidOperationException("User not found.");
    
    private AuthResponse IssueTokensAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
        };
    }

}
