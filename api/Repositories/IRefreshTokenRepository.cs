using Passly.Entities;

namespace Passly.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(object refreshToken, object ct);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(Guid userId, CancellationToken ct = default);

}