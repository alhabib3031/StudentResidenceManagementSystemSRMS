using SRMS.Domain.Students.Enums;

namespace SRMS.Application.Identity.DTOs;

public class CompleteStudentProfileDto
{
    // Personal & Identity
    public string NationalId { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // Academic Information
    public string UniversityName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public Guid CollegeId { get; set; }
    public int AcademicYear { get; set; }
    public string AcademicTerm { get; set; } = string.Empty;

    // Contact & Address
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "Libya";
    public string? PostalCode { get; set; }

    // Emergency Contact
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string EmergencyContactRelation { get; set; } = string.Empty;

    // Document Paths (Set after upload)
    public string? BirthCertificatePath { get; set; }
    public string? HighSchoolCertificatePath { get; set; }
    public string? HealthCertificatePath { get; set; }
    public string? ResidencePermitPath { get; set; }
    public string? ProfilePicturePath { get; set; }
}
