using Passly.Entities;
using Passly.Repositories;

namespace Passly.Services.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _logger = logger;
    }
    
    public async Task<User> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetUserByIdAsync(id, ct);
        if (user is null)
        {
            _logger.LogWarning("User not found for ID {UserId}", id);
            throw new InvalidOperationException("User not found.");
        }

        return user;
    }

    public async Task<User> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        email = NormalizeEmail(email);

        var user = await _userRepository.GetUserByEmailAsync(email, ct);
        if (user is null)
        {
            _logger.LogWarning("User not found for email {Email}", email);
            throw new InvalidOperationException("User not found.");
        }

        return user;
    }

    public async Task ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        
        var valid = await _passwordService.VerifyPasswordAsync(user, currentPassword, ct);
        
        if (!valid)
        {
            _logger.LogWarning("User {UserId} failed password change attempt (wrong current password)", id);
            throw new UnauthorizedAccessException("Invalid current password.");
        }
        
        var (hash, salt) = await _passwordService.HashPasswordAsync(newPassword, ct);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);
        _logger.LogInformation("User {UserId} successfully changed password", id);
        
    }

    public async Task<bool> IsEmailVerifiedAsync(Guid id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        return user.EmailVerified;
    }

    public async Task DeleteAccountAsync(Guid id, CancellationToken ct = default)
    {
        await _userRepository.DeleteAsync(id, ct);
        _logger.LogInformation("User {UserId} deleted their account", id);
    }
    
    private static string NormalizeEmail(string email)
        => email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
}