using SRMS.Domain.Abstractions;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Managers;

/// <summary>
/// Manager - مدير السكن
/// </summary>
public class Manager : Entity
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    
    // Contact Information
    public Email? Email { get; set; } = null!;
    public PhoneNumber? PhoneNumber { get; set; }
    public Address? Address { get; set; }
    
    // Profile
    public string? ImagePath { get; private set; }
    public string? EmployeeNumber { get; private set; }
    public DateTime? HireDate { get; private set; }
    
    // Assigned Residences
    private readonly List<Residences.Residence> _residences = new();
    public IReadOnlyCollection<Residences.Residence> Residences => _residences.AsReadOnly();
    
    // Managed Students
    private readonly List<Student> _students = new();
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();
    
    // Working Hours
    public TimeSpan? WorkingHoursStart { get; private set; }
    public TimeSpan? WorkingHoursEnd { get; private set; }
    
    // Status
    public ManagerStatus Status { get; private set; }
    
    private Manager() { }
    
    public static Manager Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required");
        
        return new Manager
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            Status = ManagerStatus.Active,
            HireDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
    }
    
    /// <summary>
    /// تحديث المعلومات
    /// </summary>
    public void UpdateInfo(
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
    /// تعيين ساعات العمل
    /// </summary>
    public void SetWorkingHours(TimeSpan start, TimeSpan end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time");
        
        WorkingHoursStart = start;
        WorkingHoursEnd = end;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تعيين رقم الموظف
    /// </summary>
    public void AssignEmployeeNumber(string employeeNumber)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Employee number cannot be empty");
        
        EmployeeNumber = employeeNumber;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تعليق المدير
    /// </summary>
    public void Suspend()
    {
        Status = ManagerStatus.Suspended;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// إعادة تفعيل المدير
    /// </summary>
    public void Reactivate()
    {
        Status = ManagerStatus.Active;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}