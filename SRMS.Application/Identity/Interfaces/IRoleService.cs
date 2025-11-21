namespace SRMS.Application.Identity.Interfaces;

public interface IRoleService
{
    Task<bool> CreateRoleAsync(string roleName, string? description = null);
    Task<bool> DeleteRoleAsync(string roleName);
    Task<List<string>> GetAllRolesAsync(bool log = true);
    Task<bool> RoleExistsAsync(string roleName);
    Task<bool> UpdateRoleAsync(string roleName, string? description);
}