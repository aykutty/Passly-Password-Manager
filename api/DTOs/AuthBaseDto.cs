using System.ComponentModel.DataAnnotations;

namespace Passly.DTOs;

public abstract class AuthBaseDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(8)]
    public string Password { get; set; } = null!;
}