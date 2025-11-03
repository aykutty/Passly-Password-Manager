
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

    public Task<User> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task AddUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsExistsByEmailAsync(String email, CancellationToken ct)
    {
        return await _dbContext.Users.AnyAsync(x => x.Email == email);
    }

    public Task<User> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}