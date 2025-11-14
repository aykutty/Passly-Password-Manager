namespace Passly.Entities;

public class VaultItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public string Url { get; set; } = null!;
    public string EncryptedPayload { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}