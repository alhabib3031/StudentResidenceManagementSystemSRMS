using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Residences.DTOs;

// ============================================================
// DTO للتحديث
// ============================================================
public class UpdateResidenceDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    // Address
    [Required(ErrorMessage = "City is required")]
    public string AddressCity { get; set; } = string.Empty;
    
    public string? AddressStreet { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; } = "Libya";

    // Capacity
    [Required]
    [Range(1, 10000)]
    public int TotalCapacity { get; set; }
    
    [Range(0, 10000)]
    public int AvailableCapacity { get; set; }

    // Pricing
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyRentAmount { get; set; }
    
    public string MonthlyRentCurrency { get; set; } = "LYD";

    // Manager
    public Guid? ManagerId { get; set; }

    // Facilities
    public bool HasWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasLaundry { get; set; }
    public bool HasGym { get; set; }
    public bool HasSwimmingPool { get; set; }
    public bool HasSecurity { get; set; }
    public bool HasKitchen { get; set; }
    public bool HasStudyRoom { get; set; }
}