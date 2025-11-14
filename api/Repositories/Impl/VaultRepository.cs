using Microsoft.EntityFrameworkCore;
using Passly.Domain;
using Passly.Entities;

namespace Passly.Repositories.Impl;

public class VaultRepository : IVaultRepository
{

    private readonly ApplicationDbContext _dbContext;

    public VaultRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(VaultItem item)
    {
        await _dbContext.VaultItems.AddAsync(item);
        await _dbContext.SaveChangesAsync();
    }
    

    public async Task<VaultItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.VaultItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserId == userId,
                ct
            );
    }

    public async Task<List<VaultItem>> GetAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.VaultItems
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(VaultItem item)
    {
        _dbContext.VaultItems.Update(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var entity = await _dbContext.VaultItems
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (entity == null)
            return; 

        _dbContext.VaultItems.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}