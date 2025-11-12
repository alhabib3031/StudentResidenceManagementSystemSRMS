using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class AuthService : IAuthService
{
    public Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LogoutAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VerifyEmailAsync(VerifyEmailDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponseDto> GoogleLoginAsync(GoogleAuthDto dto)
    {
        throw new NotImplementedException();
    }
}