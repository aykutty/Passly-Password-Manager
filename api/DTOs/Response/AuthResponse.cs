namespace Passly.DTOs.Response;

public class AuthResponse
{
    public required string AccessToken { get; init; }
    public required DateTime AccessTokenExpiresAtUtc { get; init; }
}