using System.ComponentModel.DataAnnotations;

namespace Passly.DTOs.Request;

public class VerifyOtpRequestDto
{
    [Required]
    public string Email { get; set; }  
    
    [Required(ErrorMessage = "Otp code is required.")]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$")]
    public string OtpCode { get; set; } 
}