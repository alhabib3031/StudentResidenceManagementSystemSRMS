using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class UserService : IUserService
{
    public Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<UserDto?> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<List<UserDto>> GetAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeactivateUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ActivateUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddToRoleAsync(Guid userId, string role)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveFromRoleAsync(Guid userId, string role)
    {
        throw new NotImplementedException();
    }
}