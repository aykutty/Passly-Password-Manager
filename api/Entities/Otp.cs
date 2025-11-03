using Passly.Enums;

namespace Passly.Entities;

public class Otp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    public string Email { get; set; } = string.Empty; 

    public string OtpCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime Expiration { get; set; }
    public bool IsUsed { get; set; } = false;
    public int AttemptCount { get; set; }

    public OtpPurpose Purpose { get; set; }
}