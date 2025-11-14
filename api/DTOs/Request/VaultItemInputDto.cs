namespace Passly.DTOs.Request;

public class VaultItemInputDto
{
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public string Url { get; set; } = null!;
    public string EncryptedPayload { get; set; } = null!;
}