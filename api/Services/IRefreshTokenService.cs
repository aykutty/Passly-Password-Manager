using Passly.Entities;

namespace Passly.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(User user, CancellationToken ct = default);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken ct = default);
    Task<string> RotateRefreshTokenAsync(RefreshToken oldToken, CancellationToken ct = default);
    public Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
}