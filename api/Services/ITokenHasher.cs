namespace Passly.Services;

public interface ITokenHasher
{
    string HashToken(string token);
    bool VerifyToken(string plainToken, string storedHash);
}