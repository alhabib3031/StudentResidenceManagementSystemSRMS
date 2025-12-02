using System.ComponentModel.DataAnnotations;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Managers.DTOs;

// ============================================================
// DTO للإنشاء
// ============================================================
public class CreateManagerDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }
    
    public string? PhoneCountryCode { get; set; } = "+218";

    // Address
    public string? AddressCity { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; } = "Libya";
    
    // Profile
    [StringLength(50)]
    public string? EmployeeNumber { get; set; }
    
    public DateTime? HireDate { get; set; }
    
    // Working Hours
    public TimeSpan? WorkingHoursStart { get; set; }
    public TimeSpan? WorkingHoursEnd { get; set; }
}