using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Identity.DTOs;

public class VerifyEmailDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
}