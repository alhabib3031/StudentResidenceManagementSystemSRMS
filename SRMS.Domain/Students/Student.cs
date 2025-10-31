using SRMS.Domain.Abstractions;
using SRMS.Domain.Complaints;
using SRMS.Domain.Managers;
using SRMS.Domain.Payments;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students.Enums;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Students;

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
    public string ImagePath { get; private set; } = string.Empty;
    public string NationalId { get; private set; } =  string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    
    // Academic Information
    public string? UniversityName { get; private set; }
    public string? StudentNumber { get; private set; }
    public string? Major { get; private set; }
    public int? AcademicYear { get; private set; }
    
    // Emergency Contact
    public string EmergencyContactName { get; private set; } = string.Empty;
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
    
    private Student() { }
    
    /// <summary>
    /// Factory Method لإنشاء Student
    /// </summary>
    public static Student Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber = null,
        Gender gender = Gender.Male)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required");
        
        return new Student
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            Gender = gender,
            Status = StudentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
    }
    
    /// <summary>
    /// تحديث المعلومات الشخصية
    /// </summary>
    public void UpdatePersonalInfo(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber,
        Address? address)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تحديث المعلومات الأكاديمية
    /// </summary>
    public void UpdateAcademicInfo(
        string? universityName,
        string? studentNumber,
        string? major,
        int? academicYear)
    {
        UniversityName = universityName;
        StudentNumber = studentNumber;
        Major = major;
        AcademicYear = academicYear;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تعيين غرفة للطالب
    /// </summary>
    public void AssignRoom(Guid roomId)
    {
        if (RoomId.HasValue)
            throw new InvalidOperationException("Student already assigned to a room");
        
        RoomId = roomId;
        RoomAssignedDate = DateTime.UtcNow;
        Status = StudentStatus.Accommodated;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// إلغاء تعيين الغرفة
    /// </summary>
    public void UnassignRoom()
    {
        if (!RoomId.HasValue)
            throw new InvalidOperationException("Student not assigned to any room");
        
        RoomId = null;
        RoomAssignedDate = null;
        Status = StudentStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تعيين مدير
    /// </summary>
    public void AssignManager(Guid managerId)
    {
        ManagerId = managerId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تعليق الطالب
    /// </summary>
    public void Suspend(string reason)
    {
        if (Status == StudentStatus.Suspended)
            throw new InvalidOperationException("Student already suspended");
        
        Status = StudentStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// إعادة تفعيل الطالب
    /// </summary>
    public void Reactivate()
    {
        if (Status != StudentStatus.Suspended)
            throw new InvalidOperationException("Can only reactivate suspended students");
        
        Status = RoomId.HasValue ? StudentStatus.Accommodated : StudentStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}