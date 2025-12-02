namespace SRMS.Application.Residences.DTOs;

// ============================================================
// DTO للعرض التفصيلي
// ============================================================
public class ResidenceDetailsDto
{
    public Guid Id { get; set; }
    
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    
    // Address
    public string? AddressCity { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
    public string? FullAddress { get; set; }
    
    // Capacity
    public int TotalCapacity { get; set; }
    public int AvailableCapacity { get; set; }
    public int OccupiedCapacity { get; set; }
    public bool IsFull { get; set; }
    public decimal OccupancyRate { get; set; }
    
    // Pricing
    public decimal MonthlyRentAmount { get; set; }
    public string MonthlyRentCurrency { get; set; } = "LYD";
    public string MonthlyRentFormatted { get; set; } = string.Empty;
    
    // Manager
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public string? ManagerEmail { get; set; }
    
    // Facilities
    public bool HasWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasLaundry { get; set; }
    public bool HasGym { get; set; }
    public bool HasSwimmingPool { get; set; }
    public bool HasSecurity { get; set; }
    public bool HasKitchen { get; set; }
    public bool HasStudyRoom { get; set; }
    public List<string> FacilitiesList { get; set; } = new();
    
    // Status
    public bool IsActive { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related Data Counts
    public int RoomsCount { get; set; }
    public int AvailableRoomsCount { get; set; }
    public int OccupiedRoomsCount { get; set; }
}