using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.Identity.Interfaces;
using SRMS.Domain.Identity;

namespace SRMS.Infrastructure.Services;

// ============================================================
// ROLE SERVICE
// ============================================================
public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    
    public RoleService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }
    
    public async Task<bool> CreateRoleAsync(string roleName, string? description = null)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
            return false;
        
        var role = new ApplicationRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpper(),
            Description = description,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await _roleManager.CreateAsync(role);
        
        return result.Succeeded;
    }
    
    public async Task<bool> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        
        if (role == null || role.IsSystemRole)
            return false;
        
        var result = await _roleManager.DeleteAsync(role);
        
        return result.Succeeded;
    }
    
    public async Task<List<string>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Select(r => r.Name!).ToList();
    }
    
    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }
    
    public async Task<bool> UpdateRoleAsync(string roleName, string? description)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        
        if (role == null)
            return false;
        
        role.Description = description;
        role.UpdatedAt = DateTime.UtcNow;
        
        var result = await _roleManager.UpdateAsync(role);
        
        return result.Succeeded;
    }
}