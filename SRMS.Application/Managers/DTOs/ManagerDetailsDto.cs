using SRMS.Domain.Managers.Enums;

namespace SRMS.Application.Managers.DTOs;

// ============================================================
// DTO للعرض التفصيلي
// ============================================================
public class ManagerDetailsDto
{
    public Guid Id { get; set; }
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
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
    
    // Profile
    public string? EmployeeNumber { get; set; }
    public DateTime? HireDate { get; set; }
    
    // Working Hours
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
    public string? WorkingHoursFormatted { get; set; }
    
    // Status
    public ManagerStatus Status { get; set; }
    public bool IsActive { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related Data Counts
    public int ResidencesCount { get; set; }
    public int StudentsCount { get; set; }
}