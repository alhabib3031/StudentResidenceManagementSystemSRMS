using SRMS.Application.Identity.DTOs;

namespace SRMS.Application.Identity.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<bool> LogoutAsync(Guid userId);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<bool> VerifyEmailAsync(VerifyEmailDto dto);
    Task<LoginResponseDto> GoogleLoginAsync(GoogleAuthDto dto);
}