using System.Security.Claims;
using Passly.Entities;

namespace Passly.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);


}