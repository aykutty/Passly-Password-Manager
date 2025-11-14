namespace Passly.DTOs.Response;

public class VaultItemResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Category { get; set; }
    public string Url { get; set; }
    public string EncryptedPayload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}