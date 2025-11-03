using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Passly.Entities;
using Passly.Options;

namespace Passly.Services.Impl;

public class JwtService : IJwtService
{
   private readonly JwtOptions _jwtOptions;
   private readonly ILogger<JwtService> _logger;
   private readonly SymmetricSecurityKey _securityKey;
   private readonly JwtSecurityTokenHandler _tokenHandler;

   public JwtService(IOptions<JwtOptions> jwtOptions, ILogger<JwtService> logger)
   {
      _jwtOptions = jwtOptions.Value;
      _logger = logger;
      
      var secretBytes = Convert.FromBase64String(_jwtOptions.Secret);
      _securityKey = new SymmetricSecurityKey(secretBytes);
      _tokenHandler = new JwtSecurityTokenHandler();
   }

   public string GenerateAccessToken(User user)
   {
      
      var claims = new List<Claim>
      {
         new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
         new Claim(JwtRegisteredClaimNames.Email, user.Email),
         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
         new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64),
      };

      var tokenDescriptor = new SecurityTokenDescriptor()
      {
         Subject = new ClaimsIdentity(claims),
         Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
         Issuer = _jwtOptions.Issuer,
         Audience = _jwtOptions.Audience,
         SigningCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature)
      };
      
      var token = _tokenHandler.CreateToken(tokenDescriptor);
      return _tokenHandler.WriteToken(token);
      
   }
   
}