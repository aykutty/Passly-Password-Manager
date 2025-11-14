using Microsoft.EntityFrameworkCore;
using Passly.Domain;
using Passly.Entities;
using Passly.Enums;

namespace Passly.Repositories.Impl;

public class OtpRepository : IOtpRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OtpRepository(ApplicationDbContext dbContext) => this._dbContext = dbContext;
    public async Task AddAsync(Otp otp, CancellationToken ct = default)
    { 
        ct.ThrowIfCancellationRequested();
        
        await _dbContext.Otps.AddAsync(otp, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Otp otp, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        
        _dbContext.Otps.Update(otp);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<List<Otp>> GetExpiredOtpsAsync(CancellationToken ct = default)
    {
        return await _dbContext.Otps
            .Where(o => o.Expiration <= DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task<Otp?> GetLatestValidOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Otps
            .Where(o => o.Email == email &&
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.Expiration > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task RemoveRangeAsync(IEnumerable<Otp> otps)
    {
        _dbContext.Otps.RemoveRange(otps);
        await _dbContext.SaveChangesAsync();
    }
}