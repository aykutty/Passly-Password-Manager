using Passly.Entities;
using Passly.Enums;

namespace Passly.Repositories;

public interface IOtpRepository 
{
    Task AddAsync(Otp otp, CancellationToken ct = default);
    Task UpdateAsync(Otp otp, CancellationToken ct = default);
    Task<List<Otp>> GetExpiredOtpsAsync(CancellationToken ct = default);
    Task<Otp?> GetLatestValidOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<Otp> otps);
}