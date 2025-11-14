using Passly.Entities;

namespace Passly.Repositories;

public interface IVaultRepository
{
    Task AddAsync(VaultItem item);
    Task<VaultItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<List<VaultItem>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task UpdateAsync(VaultItem item);
    Task DeleteAsync(Guid id, Guid userId);
}
