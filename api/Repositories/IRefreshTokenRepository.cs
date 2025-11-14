using Passly.Entities;

namespace Passly.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task UpdateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(Guid userId, CancellationToken ct = default);

}