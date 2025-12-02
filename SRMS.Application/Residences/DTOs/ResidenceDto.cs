namespace SRMS.Application.Residences.DTOs;

// ============================================================
// DTO للقراءة (بسيط)
// ============================================================
public class ResidenceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int TotalCapacity { get; set; }
    public int AvailableCapacity { get; set; }
    public bool IsFull { get; set; }
    public string? MonthlyRent { get; set; }
    public string? ManagerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}