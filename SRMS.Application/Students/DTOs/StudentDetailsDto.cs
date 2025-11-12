using SRMS.Domain.Students.Enums;

namespace SRMS.Application.Students.DTOs;

// ============================================================
// DTO للعرض التفصيلي (مع كل البيانات)
// ============================================================
public class StudentDetailsDto
{
    public Guid Id { get; set; }
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? ImagePath { get; set; }
    
    // Contact Information
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
    public string? FullAddress { get; set; }
    
    // Academic Information
    public string? UniversityName { get; set; }
    public string? StudentNumber { get; set; }
    public string? Major { get; set; }
    public int? AcademicYear { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Room Assignment
    public Guid? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public DateTime? RoomAssignedDate { get; set; }
    
    // Manager
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    
    // Status
    public StudentStatus Status { get; set; }
    public bool IsActive { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related Data Counts
    public int PaymentsCount { get; set; }
    public int ComplaintsCount { get; set; }
}