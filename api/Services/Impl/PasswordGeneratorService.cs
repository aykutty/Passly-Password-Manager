using System.Security.Cryptography;
using System.Text;
using Passly.DTOs.Request;

namespace Passly.Services.Impl;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Numbers = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?";

    private static readonly char[] SimilarChars = { 'O', '0', 'I', 'l' };
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public string Generate(PasswordGenerationRequest req)
    {
        if (req.Length < 8 || req.Length > 128)
            throw new ArgumentException("Password length must be between 8 and 128.");

        var pool = new StringBuilder();

        if (req.IncludeLowercase) pool.Append(Lowercase);
        if (req.IncludeUppercase) pool.Append(Uppercase);
        if (req.IncludeNumbers) pool.Append(Numbers);
        if (req.IncludeSymbols) pool.Append(Symbols);

        if (pool.Length == 0)
            throw new InvalidOperationException("No character sets selected.");

        var chars = pool.ToString();

        if (req.ExcludeSimilar)
            chars = new string(chars.Except(SimilarChars).ToArray());

        var result = new char[req.Length];
        var buffer = new byte[sizeof(uint)];

        for (int i = 0; i < req.Length; i++)
        {
            _rng.GetBytes(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            result[i] = chars[(int)(num % chars.Length)];
        }

        return new string(result);
    }
}