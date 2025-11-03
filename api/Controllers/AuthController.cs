using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Passly.DTOs.Request;
using Passly.Services;

namespace Passly.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService,
        IUserService userService,
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        try
        {
            await _authService.RegisterAsync(dto.Email, dto.Password, ct);
            return Ok(new { Message = "Registration successful. Please verify your email." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed for email {Email}", dto.Email);
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);
            return StatusCode(500, new { Message = "Internal server error." });
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        try
        {
            var authResponse = await _authService.LoginWithPasswordAsync(dto.Email, dto.Password, ct);
            
            Response.Cookies.Append("AccessToken", authResponse.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.AccessTokenExpiresAtUtc
            });

            return Ok(new { Message = "Login successful" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", dto.Email);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
    
    

    
}