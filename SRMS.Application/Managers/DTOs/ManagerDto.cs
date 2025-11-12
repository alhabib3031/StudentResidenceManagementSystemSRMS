using SRMS.Domain.Managers.Enums;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Managers.DTOs;

// ============================================================
// DTO للقراءة (بسيط)
// ============================================================
public class ManagerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmployeeNumber { get; set; }
    public ManagerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}