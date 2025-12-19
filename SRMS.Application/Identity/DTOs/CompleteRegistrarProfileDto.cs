namespace SRMS.Application.Identity.DTOs;

public class CompleteRegistrarProfileDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid CollegeId { get; set; }

    // Additional info that might be needed
    public string? ProfilePicturePath { get; set; }
    public string? PhoneNumber { get; set; }
}
