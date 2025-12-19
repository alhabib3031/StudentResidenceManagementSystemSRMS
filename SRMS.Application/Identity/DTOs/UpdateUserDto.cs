using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Identity.DTOs;

public class UpdateUserDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Theme { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool SMSNotificationsEnabled { get; set; }
    public string? ProfilePicture { get; set; }
}