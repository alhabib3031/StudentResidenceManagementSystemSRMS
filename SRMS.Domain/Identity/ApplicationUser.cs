using Microsoft.AspNetCore.Identity;
using SRMS.Domain.Identity.Enums;

namespace SRMS.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    // Profile
    public string? ProfilePicture { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; }

    // Additional Contact
    public string? AlternativeEmail { get; set; }
    public string? AlternativePhone { get; set; }

    // Address
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "Libya";

    // Account Status
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Profile Status
    public UserProfileStatus ProfileStatus { get; set; } = UserProfileStatus.PendingAccountTypeSelection;
    public string? RejectionReason { get; set; }

    // Related Entities (optional)
    public Guid? StudentId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? RegistrarId { get; set; }

    // Google OAuth
    public string? GoogleId { get; set; }
    public string? GoogleAccessToken { get; set; }
    public string? GoogleRefreshToken { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; } = "ar";
    public string? Theme { get; set; } = "light";
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SMSNotificationsEnabled { get; set; } = true;
}