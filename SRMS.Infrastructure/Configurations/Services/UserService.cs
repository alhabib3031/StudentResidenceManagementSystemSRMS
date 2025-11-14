using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Identity;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;

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
    
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return null;
        
        var roles = await _userManager.GetRolesAsync(user);
        
        return new UserDto
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
        };
    }
    
    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : await GetUserByIdAsync(user.Id);
    }
    
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync();
        
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            userDtos.Add(new UserDto
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
            });
        }
        
        return userDtos;
    }
    
    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.City = dto.City;
        user.Street = dto.Street;
        user.PostalCode = dto.PostalCode;
        user.Country = dto.Country;
        user.PreferredLanguage = dto.PreferredLanguage;
        user.Theme = dto.Theme;
        user.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
        user.SMSNotificationsEnabled = dto.SMSNotificationsEnabled;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }
    
    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }
    
    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }
    
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        // Soft delete
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;
        
        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }
    
    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return new List<string>();
        
        var roles = await _userManager.GetRolesAsync(user);
        
        return roles.ToList();
    }
    
    public async Task<bool> AddToRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        if (!await _roleManager.RoleExistsAsync(role))
            return false;
        
        var result = await _userManager.AddToRoleAsync(user, role);
        
        return result.Succeeded;
    }
    
    public async Task<bool> RemoveFromRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;
        
        var result = await _userManager.RemoveFromRoleAsync(user, role);
        
        return result.Succeeded;
    }
}