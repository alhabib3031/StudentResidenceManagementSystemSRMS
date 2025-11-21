using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.Identity;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Infrastructure.Configurations.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditService _audit;
    
    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IAuditService audit)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _audit = audit;
    }
    
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return null;
        
        var roles = await _userManager.GetRolesAsync(user);
        
        // ✅ Log user view (optional - might generate too many logs)
        // await _audit.LogAsync(
        //     AuditAction.Read,
        //     "User",
        //     user.Id.ToString(),
        //     additionalInfo: $"User profile viewed: {user.FullName}"
        // );
        
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
    
    public async Task<List<UserDto>> GetAllUsersAsync(bool log = true)
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

        if (log)
        {
            // ✅ Log bulk user retrieval
            await _audit.LogAsync(
                AuditAction.Read,
                "User",
                additionalInfo: $"Retrieved all users - Count: {userDtos.Count}"
            );
        }
        
        return userDtos;
    }
    
    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: "Attempted to update non-existent user"
            );
            return false;
        }
        
        // Store old values
        var oldValues = new
        {
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.City,
            user.Street,
            user.PostalCode,
            user.Country,
            user.PreferredLanguage,
            user.Theme,
            user.EmailNotificationsEnabled,
            user.SMSNotificationsEnabled
        };
        
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
        
        if (result.Succeeded)
        {
            // Store new values
            var newValues = new
            {
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.City,
                user.Street,
                user.PostalCode,
                user.Country,
                user.PreferredLanguage,
                user.Theme,
                user.EmailNotificationsEnabled,
                user.SMSNotificationsEnabled
            };
            
            // ✅ Log user update
            await _audit.LogCrudAsync(
                action: AuditAction.Update,
                oldEntity: oldValues,
                newEntity: newValues,
                additionalInfo: $"User profile updated: {user.FullName}"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: $"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
        
        return result.Succeeded;
    }
    
    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: "Attempted to deactivate non-existent user"
            );
            return false;
        }
        
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            // ✅ Log user deactivation
            await _audit.LogAsync(
                action: AuditAction.Update,
                entityName: "User",
                entityId: userId.ToString(),
                oldValues: new { IsActive = true },
                newValues: new { IsActive = false },
                additionalInfo: $"User deactivated: {user.FullName}"
            );
        }
        
        return result.Succeeded;
    }
    
    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: "Attempted to activate non-existent user"
            );
            return false;
        }
        
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            // ✅ Log user activation
            await _audit.LogAsync(
                action: AuditAction.Update,
                entityName: "User",
                entityId: userId.ToString(),
                oldValues: new { IsActive = false },
                newValues: new { IsActive = true },
                additionalInfo: $"User activated: {user.FullName}"
            );
        }
        
        return result.Succeeded;
    }
    
    public async Task<List<UserDto>> GetActiveUsersAsync(bool log = true)
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive && !u.IsDeleted)
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

    
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: "Attempted to delete non-existent user"
            );
            return false;
        }
        
        var userInfo = new
        {
            user.Id,
            user.FullName,
            user.Email,
            Roles = await _userManager.GetRolesAsync(user)
        };
        
        // Soft delete
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            // ✅ Log user deletion
            await _audit.LogCrudAsync(
                action: AuditAction.Delete,
                oldEntity: userInfo,
                additionalInfo: $"User deleted (soft delete): {userInfo.FullName}"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "User",
                userId.ToString(),
                additionalInfo: $"Failed to delete user: {userInfo.FullName}"
            );
        }
        
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
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "UserRole",
                userId.ToString(),
                additionalInfo: $"Attempted to assign role '{role}' to non-existent user"
            );
            return false;
        }
        
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "UserRole",
                userId.ToString(),
                additionalInfo: $"Attempted to assign non-existent role: {role}"
            );
            return false;
        }
        
        var result = await _userManager.AddToRoleAsync(user, role);
        
        if (result.Succeeded)
        {
            // ✅ Log role assignment
            await _audit.LogRoleChangeAsync(userId, role, isAdded: true);
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "UserRole",
                userId.ToString(),
                additionalInfo: $"Failed to assign role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
        
        return result.Succeeded;
    }
    
    public async Task<bool> RemoveFromRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "UserRole",
                userId.ToString(),
                additionalInfo: $"Attempted to remove role '{role}' from non-existent user"
            );
            return false;
        }
        
        var result = await _userManager.RemoveFromRoleAsync(user, role);
        
        if (result.Succeeded)
        {
            // ✅ Log role removal
            await _audit.LogRoleChangeAsync(userId, role, isAdded: false);
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "UserRole",
                userId.ToString(),
                additionalInfo: $"Failed to remove role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
        
        return result.Succeeded;
    }
}