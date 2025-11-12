using SRMS.Application.Identity.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class RoleService : IRoleService
{
    public Task<bool> CreateRoleAsync(string roleName, string? description = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteRoleAsync(string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetAllRolesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> RoleExistsAsync(string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateRoleAsync(string roleName, string? description)
    {
        throw new NotImplementedException();
    }
}