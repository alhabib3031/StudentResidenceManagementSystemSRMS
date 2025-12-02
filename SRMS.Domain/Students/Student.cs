using SRMS.Domain.Abstractions;
using SRMS.Domain.Managers;
using SRMS.Domain.Payments;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students.Enums;
using SRMS.Domain.ValueObjects;
using Complaint = SRMS.Domain.Complaints.Complaint;

namespace SRMS.Domain.Students;

/// <summary>
/// Student Entity - الطالب (حامل للخصائص فقط)
/// </summary>
public class Student : Entity
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
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    
    // Academic Information
    public string? UniversityName { get; set; }
    public string? StudentNumber { get; set; }
    public string? Major { get; set; }
    public int? AcademicYear { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public PhoneNumber? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Room Assignment
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }
    public DateTime? RoomAssignedDate { get; set; }
    
    // Manager
    public Guid? ManagerId { get; set; }
    public Manager? Manager { get; set; }
    
    // Navigation Properties
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    
    // Status
    public StudentStatus Status { get; set; }
}