using System.ComponentModel.DataAnnotations;
using SRMS.Domain.Students.Enums;

namespace SRMS.Application.Students.DTOs;

// ============================================================
// DTO للتحديث
// ============================================================
public class UpdateStudentDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    public string? PhoneCountryCode { get; set; } = "+218";

    // Address
    public string? AddressCity { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; } = "Libya";

    // Profile
    public string? NationalId { get; set; }
    public Guid? NationalityId { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // Academic Information
    public string? UniversityName { get; set; }
    public string? StudentNumber { get; set; }
    public string? Major { get; set; }
    public int? AcademicYear { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactPhoneCountryCode { get; set; } = "+218";
    public string? EmergencyContactRelation { get; set; }

    // Status
    public StudentStatus Status { get; set; }
}