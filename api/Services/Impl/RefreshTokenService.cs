using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Passly.Entities;
using Passly.Options;
using Passly.Repositories;

namespace Passly.Services.Impl;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly ITokenHasher _tokenHasher;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepo,
        ILogger<RefreshTokenService> logger, 
        ITokenHasher tokenHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _logger = logger;
        _tokenHasher = tokenHasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken ct = default)
    {
        var existingToken = await _refreshTokenRepo.GetActiveRefreshTokenAsync(user.Id, ct);
        if (existingToken is not null)
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepo.UpdateAsync(existingToken, ct);

            _logger.LogInformation("Revoked existing refresh token for user {UserId}", user.Id);
        }
        
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = _tokenHasher.HashToken(token);
        
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            Expiration = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null,
            ReplacedByTokenHash = null,
        };
        await _refreshTokenRepo.AddAsync(refreshToken, ct);
        _logger.LogInformation("Refresh token created for user {UserId}", user.Id);
        
        return token;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        var tokenHash = _tokenHasher.HashToken(token);
        var storedToken = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash, ct);

        if (storedToken is null)
        {
            _logger.LogWarning("Refresh token not found");
            return null;
        }

        if (storedToken.IsExpired)
        {
            _logger.LogInformation("Refresh token expired for user {UserId}", storedToken.UserId);
            return null;
        }

        if (!storedToken.IsActive)
        {
            _logger.LogInformation("Refresh token inactive for user {UserId}", storedToken.UserId);
            return null;
        }

        return storedToken;
    }

    public async Task<string> RotateRefreshTokenAsync(RefreshToken oldToken, CancellationToken ct = default)
    {
        var newPlainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newTokenHash = _tokenHasher.HashToken(newPlainToken);

        var newToken = new RefreshToken
        {
            UserId = oldToken.UserId,
            TokenHash = newTokenHash,
            Expiration = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.ReplacedByTokenHash = newTokenHash;
 
        await _refreshTokenRepo.UpdateAsync(oldToken, ct);
        await _refreshTokenRepo.AddAsync(newToken, ct);

        _logger.LogInformation("Rotated refresh token for user {UserId}", oldToken.UserId);

        return newPlainToken;
    }
    
    public async Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepo.UpdateAsync(token, ct);
        _logger.LogInformation("Revoked refresh token for user {UserId}", token.UserId);
    }
    
}