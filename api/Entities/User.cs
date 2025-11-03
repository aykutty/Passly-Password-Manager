namespace Passly.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; } = false;
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
    public int DegreeOfParallelism { get; set; }
    public int HashSize { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
    public bool LockoutEnabled { get; set; } = false;
    public int AccessFailedCount { get; set; } = 0;
    public DateTime? LastLoginAt { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
    public ICollection<Otp>? Otps { get; set; }
}