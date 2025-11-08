using SRMS.Domain.Abstractions;
using SRMS.Domain.Common.ValueObjects;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Residences;
using SRMS.Domain.Students;

namespace SRMS.Domain.Managers;

/// <summary>
/// Manager - مدير السكن
/// </summary>
public class Manager : Entity
{
    // Personal Information
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    
    // Contact Information
    public Email Email { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    
    // Profile
    public string? ImagePath { get; private set; }
    public string? EmployeeNumber { get; private set; }
    public DateTime? HireDate { get; private set; }
    
    // Assigned Residences
    private readonly List<Residence> _residences = new();
    public IReadOnlyCollection<Residence> Residences => _residences.AsReadOnly();
    
    // Managed Students
    private readonly List<Student> _students = new();
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();
    
    // Working Hours
    public TimeSpan? WorkingHoursStart { get; private set; }
    public TimeSpan? WorkingHoursEnd { get; private set; }
    
    // Status
    public ManagerStatus Status { get; private set; }
}