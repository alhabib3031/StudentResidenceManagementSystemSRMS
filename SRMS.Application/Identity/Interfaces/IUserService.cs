using SRMS.Application.Identity.DTOs;
using SRMS.Domain.Identity.Enums;
using SRMS.Domain.Students.Enums;

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
    Task<bool> UpdateProfileStatusAsync(Guid userId, UserProfileStatus status, string? rejectionReason = null);
    Task<bool> CompleteStudentProfileAsync(Guid userId, CompleteStudentProfileDto dto);
    Task<bool> CompleteRegistrarProfileAsync(Guid userId, CompleteRegistrarProfileDto dto);
    Task<bool> SetAccountTypeAsync(Guid userId, string accountType);
    Task<List<UserDto>> GetUsersByStatusAsync(UserProfileStatus status);
    Task<bool> ApproveProfileAsync(Guid userId);
    Task<bool> RejectProfileAsync(Guid userId, string reason);
    Task<bool> AcademicallyVerifyStudentAsync(Guid studentId, bool isApproved, string? notes);
    Task<UserReviewDetailsDto?> GetUserReviewDetailsAsync(Guid userId);
    Task<RegistrarReviewDto?> GetRegistrarByIdAsync(Guid registrarId);
    Task<List<StudentReviewDto>> GetStudentsByCollegeAsync(Guid collegeId);
    Task<string?> UpdateProfilePictureAsync(Guid userId, System.IO.Stream fileStream, string fileName);
    Task<int> BulkUpdateStudentStatusByCollegeAsync(Guid collegeId, StudentStatus newStatus);
}
