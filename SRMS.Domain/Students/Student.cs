using SRMS.Domain.Abstractions;

using SRMS.Domain.Payments;
using SRMS.Domain.Rooms;
using SRMS.Domain.Colleges;
using SRMS.Domain.Reservations;
using SRMS.Domain.Students.Enums;
using SRMS.Domain.ValueObjects;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Common; // Added

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

    public Guid? NationalityId { get; set; }
    public Nationality? Nationality { get; set; }

    public StudyLevel StudyLevel { get; set; }
    // Default to a value or 0. Since it's a value type, it defaults to 0. 
    // If I want it required, I should handle it. 
    // Let's assume existing data migration will handle this or it validates on Save.

    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // Required Profile Documents (PDF Paths)
    public string? BirthCertificatePath { get; set; }
    public string? HighSchoolCertificatePath { get; set; }
    public string? HealthCertificatePath { get; set; }
    public string? ResidencePermitPath { get; set; }

    // Academic Information
    public string? UniversityName { get; set; }
    public string? StudentNumber { get; set; }
    public string? Major { get; set; }

    // College Relationship
    public Guid? CollegeId { get; set; }
    public College? College { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public PhoneNumber? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Navigation Properties for M:N with Room via Reservation
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();



    // Navigation Properties
    // public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    // public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    // Academic Information (Moved from elsewhere)
    public int AcademicYear { get; set; } // Added as per request
    public string AcademicTerm { get; set; } = string.Empty; // Added as per request

    // Status
    public StudentStatus Status { get; set; }
}