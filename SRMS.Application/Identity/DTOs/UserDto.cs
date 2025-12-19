using SRMS.Domain.Identity.Enums;

namespace SRMS.Application.Identity.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public UserProfileStatus ProfileStatus { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginCount { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Theme { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool SMSNotificationsEnabled { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? RegistrarId { get; set; }
}