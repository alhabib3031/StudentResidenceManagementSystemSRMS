using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;
using SRMS.Domain.Identity;

namespace SRMS.Infrastructure.Configurations.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ════════════════════════════════════════════════════════════
    // Get User
    // ════════════════════════════════════════════════════════════

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : await MapToUserDto(user);
    }

    public async Task<List<UserDto>> GetAllUsersAsync(bool log = true)
    {
        var users = await _userManager.Users.ToListAsync();
        return await MapUsersToDtoList(users);
    }

    public async Task<List<UserDto>> GetActiveUsersAsync(bool log = true)
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive)
            .ToListAsync();

        return await MapUsersToDtoList(users);
    }

    // ════════════════════════════════════════════════════════════
    // Update User
    // ════════════════════════════════════════════════════════════

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto updateDto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.PhoneNumber = updateDto.PhoneNumber;
        user.City = updateDto.City;
        user.Country = updateDto.Country;
        user.Street = updateDto.Street;
        user.PostalCode = updateDto.PostalCode;
        user.PreferredLanguage = updateDto.PreferredLanguage;
        user.Theme = updateDto.Theme;
        user.EmailNotificationsEnabled = updateDto.EmailNotificationsEnabled;
        user.SMSNotificationsEnabled = updateDto.SMSNotificationsEnabled;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // ════════════════════════════════════════════════════════════
    // Activate / Deactivate / Delete
    // ════════════════════════════════════════════════════════════

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && (await _userManager.DeleteAsync(user)).Succeeded;
    }

    // ════════════════════════════════════════════════════════════
    // Roles Handling
    // ════════════════════════════════════════════════════════════

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? new() : (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task<bool> AddToRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !await _roleManager.RoleExistsAsync(roleName)) return false;

        return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
    }

    public async Task<bool> RemoveFromRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
    }

    // ════════════════════════════════════════════════════════════
    // Private Helpers
    // ════════════════════════════════════════════════════════════

    private async Task<UserDto> MapToUserDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            ProfilePicture = user.ProfilePicture,
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LoginCount = user.LoginCount,
            City = user.City,
            Country = user.Country,
            Street = user.Street,
            PostalCode = user.PostalCode,
            PreferredLanguage = user.PreferredLanguage,
            Theme = user.Theme,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled,
            SMSNotificationsEnabled = user.SMSNotificationsEnabled
        };
    }

    private async Task<List<UserDto>> MapUsersToDtoList(List<ApplicationUser> users)
    {
        var result = new List<UserDto>();
        foreach (var user in users)
            result.Add(await MapToUserDto(user));
        return result;
    }
}
