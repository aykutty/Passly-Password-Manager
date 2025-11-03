namespace Passly.Options;

public class OtpOptions
{
    public int MaxAttempts { get; set; }= 3;
    public int ExpiryMinutes { get; set; } = 5;
    public int Length { get; set; } = 6;
}