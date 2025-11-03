using Passly.Entities;

namespace Passly.Services;

public interface IPasswordService
{
    Task<(byte[] Hash, byte[] Salt)> HashPasswordAsync(string password, CancellationToken ct = default);
    Task<bool> VerifyPasswordAsync(User user, string password, CancellationToken ct = default);
    byte[] GenerateSalt(int size);
    Task PerformDummyHashAsync(string email, string password, CancellationToken ct = default);
}