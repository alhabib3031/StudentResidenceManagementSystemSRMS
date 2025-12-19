using SRMS.Application.Students.DTOs;

namespace SRMS.Application.Identity.DTOs;

public class UserReviewDetailsDto
{
    public UserDto User { get; set; } = null!;
    public StudentReviewDto? StudentDetails { get; set; }
    public RegistrarReviewDto? RegistrarDetails { get; set; }
}

public class StudentReviewDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
    public string? AcademicTerm { get; set; }
    public string? BirthCertificatePath { get; set; }
    public string? HighSchoolCertificatePath { get; set; }
    public string? HealthCertificatePath { get; set; }
    public string? ResidencePermitPath { get; set; }
    public string? NationalId { get; set; }
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public SRMS.Domain.Students.Enums.StudentStatus Status { get; set; }
}

public class RegistrarReviewDto
{
    public Guid CollegeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
}
