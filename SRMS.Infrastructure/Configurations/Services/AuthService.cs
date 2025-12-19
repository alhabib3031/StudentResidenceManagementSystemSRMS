using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Constants;
using SRMS.Domain.Identity.Enums;

namespace SRMS.Infrastructure.Configurations.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _audit;
    private readonly IUserService _userService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IConfiguration configuration,
        IAuditService audit,
        IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _configuration = configuration;
        _audit = audit;
        _userService = userService;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                additionalInfo: $"Registration failed - Email already exists: {dto.Email}"
            );

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
            IsActive = true,
            ProfileStatus = UserProfileStatus.PendingAccountTypeSelection
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                additionalInfo: $"Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );

            return new LoginResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Default role will be assigned after account type selection
        // await _userManager.AddToRoleAsync(user, Roles.Student);

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Send verification email (token will be URL-encoded in EmailService)
        await _emailService.SendVerificationEmailAsync(user.Email, user.Id.ToString(), encodedToken);

        // ✅ Log successful registration
        await _audit.LogCrudAsync(
            action: AuditAction.Create,
            newEntity: new
            {
                user.Id,
                user.FullName,
                user.Email,
                Role = "Student"
            },
            additionalInfo: $"New user registered: {user.FullName} ({user.Email})"
        );

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
            // ✅ Log failed login - user not found
            await _audit.LogLoginAttemptAsync(dto.Email, false, "User not found");
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        if (!user.IsActive)
        {
            // ✅ Log failed login - account inactive
            await _audit.LogLoginAttemptAsync(dto.Email, false, "Account is deactivated");
            return new LoginResponseDto
            {
                Success = false,
                Message = "Account is deactivated. Please contact support."
            };
        }

        if (!user.EmailConfirmed)
        {
            // ✅ Log failed login - email not verified
            await _audit.LogLoginAttemptAsync(dto.Email, false, "Email not confirmed");
            return new LoginResponseDto
            {
                Success = false,
                Message = "Please verify your email before logging in",
                RequiresEmailConfirmation = true
            };
        }

        // ✅ استخدم CheckPasswordSignInAsync (لا يكتب Cookie!)
        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
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
            // ✅ Log account lockout
            await _audit.LogAsync(
                AuditAction.AccountLocked,
                "User",
                user.Id.ToString(),
                additionalInfo: "Account locked due to multiple failed login attempts"
            );

            return new LoginResponseDto
            {
                Success = false,
                Message = "Account locked due to multiple failed login attempts"
            };
        }

        if (!result.Succeeded)
        {
            // ✅ Log failed login - wrong password
            await _audit.LogLoginAttemptAsync(dto.Email, false, "Invalid password");

            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // ✅ Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.LoginCount++;
        await _userManager.UpdateAsync(user);

        // ✅ Get roles
        var roles = await _userManager.GetRolesAsync(user);

        // ✅ Log successful login
        await _audit.LogAsync(
            AuditAction.Login,
            "User",
            user.Id.ToString(),
            additionalInfo: $"Successful login - Roles: {string.Join(", ", roles)}"
        );

        // ✅ لا حاجة لـ JWT في Blazor Server، سنستخدم CustomAuthenticationStateProvider
        return new LoginResponseDto
        {
            Success = true,
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
                ProfileStatus = user.ProfileStatus,
                RejectionReason = user.RejectionReason,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LoginCount = user.LoginCount
            }
        };
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        // ✅ Log logout
        await _audit.LogAsync(
            AuditAction.Logout,
            "User",
            userId.ToString(),
            additionalInfo: $"User logged out - Roles: {string.Join(", ", user?.Roles ?? new List<string>())}"
        );

        await _signInManager.SignOutAsync();
        return true;
    }
    //
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !user.EmailConfirmed)
        {
            // Don't reveal that the user doesn't exist
            await _audit.LogAsync(
                AuditAction.LoginFailed,
                "User",
                additionalInfo: $"Password reset requested for non-existent/unconfirmed email: {dto.Email}"
            );
            return true;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        // Token will be URL-encoded in EmailService
        await _emailService.SendPasswordResetEmailAsync(user.Email!, encodedToken);

        // ✅ Log password reset request
        await _audit.LogAsync(
            AuditAction.PasswordReset,
            "User",
            user.Id.ToString(),
            additionalInfo: $"Password reset email sent to: {user.Email}"
        );

        return true;
    }
    //
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                additionalInfo: $"Password reset failed - User not found: {dto.Email}"
            );
            return false;
        }

        string decodedToken;
        try
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(dto.Token);
            decodedToken = Encoding.UTF8.GetString(decodedBytes);
        }
        catch
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                additionalInfo: $"Password reset failed - Invalid Token: {dto.Email}"
            );
            return false;
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (result.Succeeded)
        {
            // ✅ Log successful password reset
            await _audit.LogAsync(
                AuditAction.PasswordReset,
                "User",
                user.Id.ToString(),
                additionalInfo: $"Password reset successful for: {user.Email}"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                user.Id.ToString(),
                additionalInfo: $"Password reset failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

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

        if (result.Succeeded)
        {
            // ✅ Log password change
            await _audit.LogAsync(
                AuditAction.PasswordChanged,
                "User",
                userId.ToString(),
                additionalInfo: "Password changed successfully"
            );
        }
        else
        {
            // ✅ Log failed password change
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: $"Password change failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        return result.Succeeded;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());

        if (user == null)
            return false;

        // Decode the token coming from URL
        var decodedBytes = WebEncoders.Base64UrlDecode(dto.Token);
        var token = Encoding.UTF8.GetString(decodedBytes);

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await _audit.LogAsync(
                AuditAction.EmailVerified,
                "User",
                user.Id.ToString(),
                additionalInfo: "Email verified successfully"
            );

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
                // Create a new user from Google data
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
                    IsActive = true,
                    ProfileStatus = UserProfileStatus.PendingAccountTypeSelection
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    await _audit.LogAsync(
                        AuditAction.Failure,
                        "User",
                        additionalInfo: $"Google registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Failed to create user account"
                    };
                }

                // Default role will be assigned after account type selection
                // await _userManager.AddToRoleAsync(user, Roles.Student);

                // ✅ Log new user via Google
                await _audit.LogCrudAsync(
                    action: AuditAction.Create,
                    newEntity: new { user.Id, user.FullName, user.Email, Provider = "Google" },
                    additionalInfo: $"New user registered via Google: {user.FullName}"
                );
            }
            // Update Google info if not set
            else if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject;
                await _userManager.UpdateAsync(user);
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.LoginCount++;
            await _userManager.UpdateAsync(user);

            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);

            // ✅ Log successful Google login
            await _audit.LogAsync(
                AuditAction.Login,
                "User",
                user.Id.ToString(),
                additionalInfo: $"Google login successful: {user.Email} - Roles: {string.Join(", ", roles)}"
            );

            // Generate JWT token
            var jwtToken = await GenerateJwtTokenAsync(user);


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
                    ProfileStatus = user.ProfileStatus,
                    RejectionReason = user.RejectionReason,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    LoginCount = user.LoginCount
                }
            };
        }
        catch (Exception ex)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                additionalInfo: $"Google authentication failed: {ex.Message}"
            );

            return new LoginResponseDto
            {
                Success = false,
                Message = $"Google authentication failed: {ex.Message}"
            };
        }
    }
    // هنا مزال ما سجلت اكواد السجلات لتجربة السابق اولا 
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