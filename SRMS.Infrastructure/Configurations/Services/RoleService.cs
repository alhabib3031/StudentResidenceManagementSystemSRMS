using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Identity.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Identity;

namespace SRMS.Infrastructure.Configurations.Services;

// ============================================================
// ROLE SERVICE
// ============================================================
public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditService _audit;

    public RoleService(RoleManager<ApplicationRole> roleManager, IAuditService audit)
    {
        _roleManager = roleManager;
        _audit = audit;
    }

    public async Task<bool> CreateRoleAsync(string roleName, string? description = null)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                additionalInfo: $"Attempted to create existing role: {roleName}"
            );
            return false;
        }

        var role = new ApplicationRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpper(),
            Description = description,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            // ✅ Log role creation
            await _audit.LogCrudAsync(
                action: AuditAction.Create,
                newEntity: new
                {
                    role.Id,
                    role.Name,
                    role.Description,
                    role.IsSystemRole
                },
                additionalInfo: $"New role created: {roleName}"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                additionalInfo: $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        return result.Succeeded;
    }

    public async Task<bool> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                additionalInfo: $"Attempted to delete non-existent role: {roleName}"
            );
            return false;
        }

        if (role.IsSystemRole)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                role.Id.ToString(),
                additionalInfo: $"Attempted to delete system role: {roleName}"
            );
            return false;
        }

        var roleInfo = new
        {
            role.Id,
            role.Name,
            role.Description
        };

        var result = await _roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            // ✅ Log role deletion
            await _audit.LogCrudAsync(
                action: AuditAction.Delete,
                oldEntity: roleInfo,
                additionalInfo: $"Role deleted: {roleName}"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                role.Id.ToString(),
                additionalInfo: $"Failed to delete role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        return result.Succeeded;
    }

    public async Task<List<string>> GetAllRolesAsync(bool log = true)
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
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                additionalInfo: $"Attempted to update non-existent role: {roleName}"
            );
            return false;
        }

        var oldDescription = role.Description;

        role.Description = description;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            // ✅ Log role update
            await _audit.LogCrudAsync(
                action: AuditAction.Update,
                oldEntity: new { Description = oldDescription },
                newEntity: new { Description = description },
                additionalInfo: $"Role updated: {roleName}"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Role",
                role.Id.ToString(),
                additionalInfo: $"Failed to update role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        return result.Succeeded;
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return new List<string>();

        var claims = await _roleManager.GetClaimsAsync(role);
        return claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
    }

    public async Task<bool> UpdateRolePermissionsAsync(string roleName, List<string> permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return false;

        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims.Where(c => c.Type == "Permission").ToList();

        // Remove permissions that are not in the new list
        foreach (var claim in currentPermissions)
        {
            if (!permissions.Contains(claim.Value))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
        }

        // Add new permissions
        foreach (var permission in permissions)
        {
            if (!currentPermissions.Any(c => c.Value == permission))
            {
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", permission));
            }
        }

        await _audit.LogCrudAsync<object>(
            action: AuditAction.Update,
            oldEntity: new { Permissions = currentPermissions.Select(c => c.Value) },
            newEntity: new { Permissions = permissions },
            additionalInfo: $"Permissions updated for role: {roleName}"
        );

        return true;
    }
}