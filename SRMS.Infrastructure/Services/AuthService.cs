using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Constants;

namespace SRMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _configuration = configuration;
    }
    
    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Email already registered"
            };
        }
        
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            NationalId = dto.NationalId,
            DateOfBirth = dto.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        var result = await _userManager.CreateAsync(user, dto.Password);
        
        if (!result.Succeeded)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }
        
        // Assign default role
        await _userManager.AddToRoleAsync(user, Roles.Student);
        
        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        // Send confirmation email
        await _emailService.SendVerificationEmailAsync(user.Email, token);
        
        return new LoginResponseDto
        {
            Success = true,
            Message = "Registration successful. Please check your email to verify your account.",
            RequiresEmailConfirmation = true
        };
    }
    
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }
        
        if (!user.IsActive)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Account is deactivated. Please contact support."
            };
        }
        
        if (!user.EmailConfirmed)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Please verify your email before logging in",
                RequiresEmailConfirmation = true
            };
        }
        
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            dto.Password,
            dto.RememberMe,
            lockoutOnFailure: true
        );
        
        if (result.RequiresTwoFactor)
        {
            return new LoginResponseDto
            {
                Success = false,
                RequiresTwoFactor = true,
                Message = "Two-factor authentication required"
            };
        }
        
        if (result.IsLockedOut)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Account locked due to multiple failed login attempts"
            };
        }
        
        if (!result.Succeeded)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }
        
        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.LoginCount++;
        await _userManager.UpdateAsync(user);
        
        // Generate JWT token
        var jwtToken = await GenerateJwtTokenAsync(user);
        
        var roles = await _userManager.GetRolesAsync(user);
        
        return new LoginResponseDto
        {
            Success = true,
            Token = jwtToken,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LoginCount = user.LoginCount
            }
        };
    }
    
    public async Task<bool> LogoutAsync(Guid userId)
    {
        await _signInManager.SignOutAsync();
        return true;
    }
    
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null || !user.EmailConfirmed)
        {
            // Don't reveal that the user doesn't exist or is not confirmed
            return true;
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        await _emailService.SendPasswordResetEmailAsync(user.Email!, token);
        
        return true;
    }
    
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
            return false;
        
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        
        return result.Succeeded;
    }
    
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword
        );
        
        return result.Succeeded;
    }
    
    public async Task<bool> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        
        if (user == null)
            return false;
        
        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
        
        if (result.Succeeded)
        {
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName);
        }
        
        return result.Succeeded;
    }
    
    public async Task<LoginResponseDto> GoogleLoginAsync(GoogleAuthDto dto)
    {
        try
        {
            // Verify Google token
            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.GoogleToken);
            
            var user = await _userManager.FindByEmailAsync(payload.Email);
            
            if (user == null)
            {
                // Create new user from Google data
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    ProfilePicture = payload.Picture,
                    GoogleId = payload.Subject,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                var result = await _userManager.CreateAsync(user);
                
                if (!result.Succeeded)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Failed to create user account"
                    };
                }
                
                await _userManager.AddToRoleAsync(user, Roles.Student);
            }
            else
            {
                // Update Google info if not set
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = payload.Subject;
                    await _userManager.UpdateAsync(user);
                }
            }
            
            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.LoginCount++;
            await _userManager.UpdateAsync(user);
            
            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            // Generate JWT token
            var jwtToken = await GenerateJwtTokenAsync(user);
            
            var roles = await _userManager.GetRolesAsync(user);
            
            return new LoginResponseDto
            {
                Success = true,
                Token = jwtToken,
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePicture = user.ProfilePicture,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    LoginCount = user.LoginCount
                }
            };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Google authentication failed: {ex.Message}"
            };
        }
    }
    
    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere123456789!")
        );
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}