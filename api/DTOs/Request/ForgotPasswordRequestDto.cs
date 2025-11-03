using System.ComponentModel.DataAnnotations;

namespace Passly.DTOs.Request;

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}