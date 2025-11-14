using Passly.DTOs.Request;

namespace Passly.Services;

public interface IPasswordGeneratorService
{
    public string Generate(PasswordGenerationRequest req);
}