namespace Passly.DTOs.Request;

public class PasswordGenerationRequest
{
    public int Length { get; set; } = 16;
    public bool IncludeLowercase { get; set; } = true;
    public bool IncludeUppercase { get; set; } = true;
    public bool IncludeNumbers { get; set; } = true;
    public bool IncludeSymbols { get; set; } = true;
    public bool ExcludeSimilar { get; set; } = false;
}