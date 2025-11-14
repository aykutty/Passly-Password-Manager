
using Passly.Entities;

namespace Passly.Repositories;

public interface IUserRepository 
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task AddUserAsync(User user);
    Task<bool> IsExistsByEmailAsync(String email, CancellationToken ct = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, CancellationToken ct = default);

}