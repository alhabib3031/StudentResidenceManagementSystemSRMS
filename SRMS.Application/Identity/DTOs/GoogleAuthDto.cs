using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Identity.DTOs;

public class GoogleAuthDto
{
    [Required]
    public string GoogleToken { get; set; } = string.Empty;
}