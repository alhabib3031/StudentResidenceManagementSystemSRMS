using SRMS.Domain.Abstractions;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Residences;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Managers;

/// <summary>
/// Manager Entity - مدير السكن (حامل للخصائص فقط)
/// </summary>
public class Manager : Entity
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    
    // Contact Information
    public Email? Email { get; set; }
    public PhoneNumber? PhoneNumber { get; set; }
    public Address? Address { get; set; }
    
    // Profile
    public string? ImagePath { get; set; }
    public string? EmployeeNumber { get; set; }
    public DateTime? HireDate { get; set; }
    
    // Working Hours
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
    
    // Status
    public ManagerStatus Status { get; set; }
    
    // Navigation Properties
    public ICollection<Residence> Residences { get; set; } = new List<Residence>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}