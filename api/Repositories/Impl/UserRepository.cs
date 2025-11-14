using Microsoft.EntityFrameworkCore;
using Passly.Domain;
using Passly.Entities;

namespace Passly.Repositories.Impl;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        return await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsExistsByEmailAsync(String email, CancellationToken ct)
    {
        return await _dbContext.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FindAsync([userId], ct);
        if (user is null)
            return;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(ct);
    }
}