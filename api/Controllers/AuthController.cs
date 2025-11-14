using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passly.DTOs.Request;
using Passly.DTOs.Response;
using Passly.Options;
using Passly.Services;

namespace Passly.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IUserService userService,
        IAuthService authService,
        ILogger<AuthController> logger,
        IOptions<JwtOptions> jwtOptions)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
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
        var user = await _userService.GetByEmailAsync(dto.Email, ct);
        if (user is null)
            return Unauthorized(new { Message = "Invalid credentials." });

        if (!user.EmailVerified)
            return BadRequest(new { Message = "Please verify your email before logging in." });

        AuthResponse? authResponse;
        try
        {
            authResponse = await _authService.LoginWithPasswordAsync(dto.Email, dto.Password, ct);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
            return StatusCode(500, new { Message = "Internal server error." });
        }
        
        Response.Cookies.Append("AccessToken", authResponse.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = authResponse.AccessTokenExpiresAtUtc
        });

        Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth/refresh",
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
        });
        
        return Ok(new
        {
            Message = "Login successful.",
            AccessToken = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken,
            AccessTokenExpiresAtUtc = authResponse.AccessTokenExpiresAtUtc
        });
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { Message = "No refresh token provided." });
        
        try
        {
            var authResponse = await _authService.RefreshAsync(refreshToken, ct);

            Response.Cookies.Append("AccessToken", authResponse.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.AccessTokenExpiresAtUtc
            });

            return Ok(new { Message = "Access token refreshed." });
        }
        
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Invalid or expired refresh token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
    
    [HttpPost("request-email-verification")]
    public async Task<IActionResult> RequestEmailVerification([FromBody] Guid userId, CancellationToken ct)
    {
        await _authService.RequestEmailVerificationAsync(userId, ct);
        return Ok(new { Message = "Verification code sent to your email." });
    }
    
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequestDto dto, CancellationToken ct)
    {
        var user = await _userService.GetByEmailAsync(dto.Email, ct);
        if (user is null) return NotFound(new { Message = "User not found." });

        try
        {
            await _authService.VerifyEmailAsync(user.Id, dto.OtpCode, ct);
            return Ok(new { Message = "Email verified successfully." });
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest(new { Message = "Invalid or expired verification code." });
        }
    }

    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
        {
            await _authService.LogoutAsync(refreshToken, ct);
        }

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/api/auth/refresh" });

        return Ok(new { Message = "Logged out successfully." });
    }




}