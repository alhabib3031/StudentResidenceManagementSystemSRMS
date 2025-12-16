using SRMS.Domain.Abstractions;
using SRMS.Domain.Managers;
using SRMS.Domain.Rooms;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Residences;

/// <summary>
/// Residence Entity - السكن (حامل للخصائص فقط)
/// </summary>
public class Residence : Entity
{
    public string Name { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public string? ImagePath { get; set; }
    public string? Description { get; set; }
    
    // Capacity
    public int TotalCapacity { get; set; }
    public int AvailableCapacity { get; set; }
    public bool IsFull => AvailableCapacity <= 0;
    

    
    // Navigation Properties for M:N with Manager
    public ICollection<ResidenceManager> ResidenceManagers { get; set; } = new List<ResidenceManager>();
    
    // Facilities
    public bool HasWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasLaundry { get; set; }
    public bool HasGym { get; set; }
    public bool HasSwimmingPool { get; set; }
    public bool HasSecurity { get; set; }
    public bool HasKitchen { get; set; }
    public bool HasStudyRoom { get; set; }
    
    // Navigation Properties
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
