namespace SRMS.Application.Identity.DTOs;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public string? Message { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
}
