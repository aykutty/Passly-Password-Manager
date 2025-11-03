using Passly.Enums;

namespace Passly.Entities;

public class SecurityEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    
    public SecurityEventType EventType { get; set; }
    public string? Details { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}