using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Managers.DTOs;

public class CreateManagerDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }
}