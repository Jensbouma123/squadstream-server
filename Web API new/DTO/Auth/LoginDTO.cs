using System.ComponentModel.DataAnnotations;

namespace Web_API_new.DTOs;

public class LoginDTO
{
    [Required]
    [MaxLength(128)]
    public string Email { get; set; }
    
    [Required]
    [MaxLength(128)]
    public string Password { get; set; }
    
    [MaxLength(6)]
    public string? TwoFaCode { get; set; }
}