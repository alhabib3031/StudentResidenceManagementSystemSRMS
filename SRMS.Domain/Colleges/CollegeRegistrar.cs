using SRMS.Domain.Abstractions;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Colleges;

/// <summary>
/// College Registrar Entity - مسجل الكلية
/// </summary>
public class CollegeRegistrar : Entity
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    // Contact Information
    public Email? Email { get; set; }
    public PhoneNumber? PhoneNumber { get; set; }

    // Registrar Specific
    public string? EmployeeNumber { get; set; } // رقم المسجل الوظيفي
    public bool IsApproved { get; set; } = false; // Account status

    // College Relationship
    public Guid? CollegeId { get; set; }
    public College? College { get; set; }
}
