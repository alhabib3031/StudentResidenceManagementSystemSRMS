using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Enums;
using SRMS.Domain.Students;
using SRMS.Domain.Students.Enums;
using SRMS.Domain.Colleges;
using SRMS.Domain.ValueObjects;
using SRMS.Application.Common.Interfaces;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Application.Notifications.DTOs;
using SRMS.Domain.Notifications.Enums;

namespace SRMS.Infrastructure.Configurations.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notificationService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IFileStorageService fileStorage,
        INotificationService notificationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _contextFactory = contextFactory;
        _fileStorage = fileStorage;
        _notificationService = notificationService;
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
        user.ProfilePicture = updateDto.ProfilePicture;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<string?> UpdateProfilePictureAsync(Guid userId, System.IO.Stream fileStream, string fileName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var path = await _fileStorage.SaveFileAsync(fileStream, fileName, "profiles");
        user.ProfilePicture = path;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? path : null;
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

    public async Task<bool> UpdateProfileStatusAsync(Guid userId, UserProfileStatus status, string? rejectionReason = null)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.ProfileStatus = status;
        user.RejectionReason = rejectionReason;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> CompleteStudentProfileAsync(Guid userId, CompleteStudentProfileDto dto)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var student = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = Email.Create(user.Email!),
                PhoneNumber = !string.IsNullOrEmpty(user.PhoneNumber) ? PhoneNumber.Create(user.PhoneNumber) : (!string.IsNullOrEmpty(dto.EmergencyContactPhone) ? PhoneNumber.Create(dto.EmergencyContactPhone) : null),
                NationalId = dto.NationalId,
                Nationality = dto.Nationality,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                UniversityName = dto.UniversityName,
                StudentNumber = dto.StudentNumber,
                Major = dto.Major,
                StudyLevel = dto.StudyLevel,
                CollegeId = dto.CollegeId != Guid.Empty ? dto.CollegeId : null,
                AcademicYear = dto.AcademicYear,
                AcademicTerm = dto.AcademicTerm ?? "",
                Address = Address.Create(dto.City ?? "", dto.Street ?? "", "", dto.PostalCode ?? "", dto.Country ?? ""),
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = !string.IsNullOrEmpty(dto.EmergencyContactPhone) ? PhoneNumber.Create(dto.EmergencyContactPhone) : null,
                EmergencyContactRelation = dto.EmergencyContactRelation,
                BirthCertificatePath = dto.BirthCertificatePath,
                HighSchoolCertificatePath = dto.HighSchoolCertificatePath,
                HealthCertificatePath = dto.HealthCertificatePath,
                ResidencePermitPath = dto.ResidencePermitPath,
                ImagePath = dto.ProfilePicturePath,
                Status = StudentStatus.ManagerApprovalPending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Students.Add(student);
            await context.SaveChangesAsync();

            user.StudentId = student.Id;
            user.ProfileStatus = UserProfileStatus.PendingApproval;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            return updateResult.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompleteStudentProfileAsync Error]: {ex.Message}");
            Console.WriteLine($"[Inner Exception]: {ex.InnerException?.Message}");
            throw;
        }
    }

    public async Task<bool> CompleteRegistrarProfileAsync(Guid userId, CompleteRegistrarProfileDto dto)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var registrar = new CollegeRegistrar
            {
                Id = Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = Email.Create(user.Email!),
                PhoneNumber = !string.IsNullOrEmpty(user.PhoneNumber) ? PhoneNumber.Create(user.PhoneNumber) : null,
                EmployeeNumber = dto.EmployeeNumber,
                CollegeId = dto.CollegeId != Guid.Empty ? dto.CollegeId : null,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.CollegeRegistrars.Add(registrar);
            await context.SaveChangesAsync();

            user.RegistrarId = registrar.Id;
            user.ProfileStatus = UserProfileStatus.PendingApproval;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            return updateResult.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompleteRegistrarProfileAsync Error]: {ex.Message}");
            Console.WriteLine($"[Inner Exception]: {ex.InnerException?.Message}");
            throw;
        }
    }

    public async Task<bool> SetAccountTypeAsync(Guid userId, string accountType)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.ProfileStatus = UserProfileStatus.PendingProfileCompletion;
        user.AccountType = accountType; // Set the account type instead of assigning a role
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<List<UserDto>> GetUsersByStatusAsync(UserProfileStatus status)
    {
        var users = await _userManager.Users
            .Where(u => u.ProfileStatus == status)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        return await MapUsersToDtoList(users);
    }

    public async Task<bool> ApproveProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.ProfileStatus = UserProfileStatus.Active;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return false;

        if (user.StudentId.HasValue)
        {
            if (!await _userManager.IsInRoleAsync(user, SRMS.Domain.Identity.Constants.Roles.Student))
                await _userManager.AddToRoleAsync(user, SRMS.Domain.Identity.Constants.Roles.Student);

            using var context = await _contextFactory.CreateDbContextAsync();
            var student = await context.Students.FindAsync(user.StudentId.Value);
            if (student != null)
            {
                student.Status = SRMS.Domain.Students.Enums.StudentStatus.Active;
                await context.SaveChangesAsync();
            }
        }
        else if (user.RegistrarId.HasValue)
        {
            if (!await _userManager.IsInRoleAsync(user, SRMS.Domain.Identity.Constants.Roles.CollegeRegistrar))
                await _userManager.AddToRoleAsync(user, SRMS.Domain.Identity.Constants.Roles.CollegeRegistrar);

            using var context = await _contextFactory.CreateDbContextAsync();
            var registrar = await context.CollegeRegistrars.FindAsync(user.RegistrarId.Value);
            if (registrar != null)
            {
                registrar.IsApproved = true;
                await context.SaveChangesAsync();
            }
        }

        // Send Notification
        await _notificationService.SendNotificationAsync(new CreateNotificationDto
        {
            Title = "Profile Approved",
            Message = "Your profile has been approved. Welcome to SRMS!",
            Type = NotificationType.System,
            UserId = user.Id
        });

        return true;
    }

    public async Task<bool> RejectProfileAsync(Guid userId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.ProfileStatus = UserProfileStatus.Rejected;
        user.RejectionReason = reason;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return false;

        // Sync with related entities
        if (user.StudentId.HasValue)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var student = await context.Students.FindAsync(user.StudentId.Value);
            if (student != null)
            {
                student.Status = SRMS.Domain.Students.Enums.StudentStatus.Rejected;
                await context.SaveChangesAsync();
            }
        }
        else if (user.RegistrarId.HasValue)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var registrar = await context.CollegeRegistrars.FindAsync(user.RegistrarId.Value);
            if (registrar != null)
            {
                registrar.IsApproved = false;
                await context.SaveChangesAsync();
            }
        }

        // Send Notification
        await _notificationService.SendNotificationAsync(new CreateNotificationDto
        {
            Title = "Profile Rejected",
            Message = $"Your profile application was rejected. Reason: {reason}",
            Type = NotificationType.System,
            UserId = userId
        });

        return true;
    }

    public async Task<bool> AcademicallyVerifyStudentAsync(Guid studentId, bool isApproved, string? notes)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var student = await context.Students.FindAsync(studentId);
        if (student == null) return false;

        student.Status = isApproved ? StudentStatus.Active : StudentStatus.Rejected;
        await context.SaveChangesAsync();

        // Get User to send notification
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);
        if (user != null)
        {
            var title = isApproved ? "Academic Verification Successful" : "Academic Verification Failed";
            var message = isApproved
                ? "Your academic status has been verified and your account is now active. Your room reservation is confirmed."
                : $"Your academic verification failed. Reason: {notes ?? "Not specified"}.";

            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                Title = title,
                Message = message,
                Type = NotificationType.System,
                UserId = user.Id
            });
        }

        return true;
    }

    public async Task<UserReviewDetailsDto?> GetUserReviewDetailsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var dto = new UserReviewDetailsDto
        {
            User = await MapToUserDto(user)
        };

        using var context = await _contextFactory.CreateDbContextAsync();

        if (user.StudentId.HasValue)
        {
            var student = await context.Students
                .Include(s => s.College)
                .FirstOrDefaultAsync(s => s.Id == user.StudentId.Value);

            if (student != null)
            {
                dto.StudentDetails = new StudentReviewDto
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    StudentNumber = student.StudentNumber ?? string.Empty,
                    UniversityName = student.UniversityName ?? string.Empty,
                    Major = student.Major ?? string.Empty,
                    CollegeName = student.College?.Name ?? "N/A",
                    AcademicYear = student.AcademicYear,
                    AcademicTerm = student.AcademicTerm,
                    BirthCertificatePath = student.BirthCertificatePath,
                    HighSchoolCertificatePath = student.HighSchoolCertificatePath,
                    HealthCertificatePath = student.HealthCertificatePath,
                    ResidencePermitPath = student.ResidencePermitPath,
                    NationalId = student.NationalId,
                    Nationality = student.Nationality?.ToString(),
                    DateOfBirth = student.DateOfBirth,
                    Status = student.Status
                };
            }
        }
        else if (user.RegistrarId.HasValue)
        {
            var registrar = await context.CollegeRegistrars
                .Include(r => r.College)
                .FirstOrDefaultAsync(r => r.Id == user.RegistrarId.Value);

            if (registrar != null)
            {
                dto.RegistrarDetails = new RegistrarReviewDto
                {
                    CollegeId = registrar.CollegeId ?? Guid.Empty,
                    EmployeeNumber = registrar.EmployeeNumber ?? string.Empty,
                    CollegeName = registrar.College?.Name ?? "N/A"
                };
            }
        }

        return dto;
    }

    public async Task<RegistrarReviewDto?> GetRegistrarByIdAsync(Guid registrarId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var registrar = await context.CollegeRegistrars
            .Include(r => r.College)
            .FirstOrDefaultAsync(r => r.Id == registrarId);

        if (registrar == null) return null;

        return new RegistrarReviewDto
        {
            CollegeId = registrar.CollegeId ?? Guid.Empty,
            EmployeeNumber = registrar.EmployeeNumber ?? string.Empty,
            CollegeName = registrar.College?.Name ?? "N/A"
        };
    }

    public async Task<List<StudentReviewDto>> GetStudentsByCollegeAsync(Guid collegeId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var students = await context.Students
            .Include(s => s.College)
            .Where(s => s.CollegeId == collegeId)
            .ToListAsync();

        return students.Select(student => new StudentReviewDto
        {
            Id = student.Id,
            FullName = student.FullName,
            StudentNumber = student.StudentNumber ?? string.Empty,
            UniversityName = student.UniversityName ?? string.Empty,
            Major = student.Major ?? string.Empty,
            CollegeName = student.College?.Name ?? "N/A",
            AcademicYear = student.AcademicYear,
            AcademicTerm = student.AcademicTerm,
            BirthCertificatePath = student.BirthCertificatePath,
            HighSchoolCertificatePath = student.HighSchoolCertificatePath,
            HealthCertificatePath = student.HealthCertificatePath,
            ResidencePermitPath = student.ResidencePermitPath,
            NationalId = student.NationalId,
            Nationality = student.Nationality?.ToString(),
            DateOfBirth = student.DateOfBirth,
            Status = student.Status
        }).ToList();
    }

    public async Task<int> BulkUpdateStudentStatusByCollegeAsync(Guid collegeId, StudentStatus newStatus)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var students = await context.Students
            .Where(s => s.CollegeId == collegeId)
            .ToListAsync();

        if (!students.Any()) return 0;

        foreach (var student in students)
        {
            student.Status = newStatus;
            // Optionally, we could add logic to skip students who are already graduation/expelled etc if needed,
            // but "Bulk Update" usually implies a hard override or specific filter.
            // For "Renewing", usually we renew those who are suspended or active?
            // The user asked to "Suspend all accounts in IT College". So override seems appropriate.
        }

        await context.SaveChangesAsync();
        return students.Count;
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
            ProfileStatus = user.ProfileStatus,
            RejectionReason = user.RejectionReason,
            AccountType = user.AccountType,
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
            SMSNotificationsEnabled = user.SMSNotificationsEnabled,
            StudentId = user.StudentId,
            RegistrarId = user.RegistrarId
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
