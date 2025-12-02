using SRMS.Application.Identity.DTOs;

namespace SRMS.Application.Identity.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<List<UserDto>> GetAllUsersAsync(bool log = true);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<List<UserDto>> GetActiveUsersAsync(bool log = true);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<bool> AddToRoleAsync(Guid userId, string role);
    Task<bool> RemoveFromRoleAsync(Guid userId, string role);
}
