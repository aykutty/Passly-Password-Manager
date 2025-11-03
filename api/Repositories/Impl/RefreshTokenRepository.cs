using Passly.Entities;

namespace Passly.Repositories.Impl;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    public Task AddAsync(object refreshToken, object ct)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshToken?> GetActiveRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}