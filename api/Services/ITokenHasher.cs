namespace Passly.Services;

public interface ITokenHasher
{
    string HashToken(string token);
}