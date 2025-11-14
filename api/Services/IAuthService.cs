using Passly.DTOs.Response;

namespace Passly.Services;

public interface IAuthService
{
    Task RegisterAsync(string email, string password, CancellationToken ct = default);

    Task<AuthResponse> LoginWithPasswordAsync(string email, string password, CancellationToken ct = default);

    Task RequestEmailVerificationAsync(Guid userId, CancellationToken ct = default);
    Task VerifyEmailAsync(Guid userId, string otp, CancellationToken ct = default);

    Task RequestPasswordResetAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

}