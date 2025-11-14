using Passly.Entities;

namespace Passly.Services;

public interface IUserService
{
    Task<User> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User> GetByEmailAsync(string email, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<bool> IsEmailVerifiedAsync(Guid id, CancellationToken ct = default);
    Task DeleteAccountAsync(Guid id, CancellationToken ct = default);
}