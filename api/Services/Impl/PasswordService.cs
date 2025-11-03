using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using Passly.Entities;
using Passly.Options;
using Passly.Repositories;

namespace Passly.Services.Impl;

public class PasswordService : IPasswordService
{
    private readonly PasswordHashingOptions _hashingOptions;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PasswordService> _logger;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(Math.Max(1,Environment.ProcessorCount));

    public PasswordService(
        IOptions<PasswordHashingOptions> options,
        IUserRepository userRepository,
        ILogger<PasswordService> logger)
    {
        _hashingOptions = options.Value;
        _userRepository = userRepository;
        _logger = logger;
    }

    public byte[] GenerateSalt(int size) => RandomNumberGenerator.GetBytes(size);
    
    public async Task<(byte[] Hash, byte[] Salt)> HashPasswordAsync(string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        
        var salt = GenerateSalt(_hashingOptions.SaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = _hashingOptions.DegreeOfParallelism,
            Iterations = _hashingOptions.Iterations,
            MemorySize = _hashingOptions.MemorySize
        };
        
        ct.ThrowIfCancellationRequested();
        await _semaphore.WaitAsync(ct);
        
        try
        {
            var hash = await Task.Run(() => argon2.GetBytes(_hashingOptions.HashSize));
            
            ct.ThrowIfCancellationRequested();

            return (hash, salt);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Hashing canceled before or while waiting for semaphore.");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> VerifyPasswordAsync(User user, string password, CancellationToken ct = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (user.PasswordSalt == null || user.PasswordHash == null) throw new InvalidOperationException("User hash/salt missing.");
        
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = user.PasswordSalt,
            DegreeOfParallelism = user.DegreeOfParallelism,
            Iterations = user.Iterations,
            MemorySize = user.MemorySize
        };

        ct.ThrowIfCancellationRequested();
        
        await _semaphore.WaitAsync(ct);

        try
        {

            var computedHash = await Task.Run(() => argon2.GetBytes(user.HashSize));

            ct.ThrowIfCancellationRequested();

            return CryptographicOperations.FixedTimeEquals(user.PasswordHash, computedHash);

        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Password verification was cancelled for user {UserId}", user.Id);
            throw;
        }

        finally
        {
            _semaphore.Release();
        }
    }

    public async Task PerformDummyHashAsync(string email, string password, CancellationToken ct = default)
    {
        var dummySalt = GenerateSalt(_hashingOptions.SaltSize);
        var dummyPasswordBytes = RandomNumberGenerator.GetBytes(_hashingOptions.SaltSize);
        
        using var argon2 = new Argon2id(dummyPasswordBytes)
        {
            Salt = dummySalt,
            DegreeOfParallelism = _hashingOptions.DegreeOfParallelism,
            Iterations = _hashingOptions.Iterations,
            MemorySize = _hashingOptions.MemorySize
        };
        
        ct.ThrowIfCancellationRequested();
        
        await _semaphore.WaitAsync(ct);
        try
        {
            await Task.Run(() => argon2.GetBytes(_hashingOptions.HashSize));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Dummy hash canceled.");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
}