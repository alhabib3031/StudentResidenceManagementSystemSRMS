using SRMS.Domain.Abstractions;
using SRMS.Domain.Common.ValueObjects;
using SRMS.Domain.Complaints;
using SRMS.Domain.Managers;
using SRMS.Domain.Students.Enums;

namespace SRMS.Domain.Students;

/// <summary>
/// Student Entity - الطالب
/// </summary>
public class Student : Entity
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
    public string? NationalId { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    
    // Academic Information
    public string? UniversityName { get; private set; }
    public string? StudentNumber { get; private set; }
    public string? Major { get; private set; }
    public int? AcademicYear { get; private set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; private set; }
    public PhoneNumber? EmergencyContactPhone { get; private set; }
    public string? EmergencyContactRelation { get; private set; }
    
    // Room Assignment
    public Guid? RoomId { get; private set; }
    public Room? Room { get; private set; }
    public DateTime? RoomAssignedDate { get; private set; }
    
    // Manager
    public Guid? ManagerId { get; private set; }
    public Manager? Manager { get; private set; }
    
    // Navigation Properties
    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    
    private readonly List<Complaint> _complaints = new();
    public IReadOnlyCollection<Complaint> Complaints => _complaints.AsReadOnly();
    
    // Status
    public StudentStatus Status { get; private set; }
}