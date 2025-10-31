using SRMS.Domain.Abstractions;
using SRMS.Domain.Managers;
using SRMS.Domain.Residence;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Residences;

/// <summary>
/// Residence (السكن) - Aggregate Root
/// </summary>
public class Residence : Entity
{
    // Properties الأساسية
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public string? ImagePath { get; private set; }
    public string? Description { get; private set; }
    
    // Capacity (السعة)
    private int TotalCapacity { get; set; }
    private int AvailableCapacity { get; set; }
    public bool IsFull => AvailableCapacity <= 0;
    
    // Pricing
    public Money MonthlyRent { get; private set; } = null!;
    
    // Manager
    public Guid? ManagerId { get; private set; }
    public Manager? Manager { get; private set; }
    
    // Navigation Properties
    private readonly List<Room> _rooms = new();
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();
    
    // Facilities (المرافق)
    public ResidenceFacilities Facilities { get; private set; } = new();
    
    // Private constructor لإجبار استخدام Factory Method
    private Residence() { }
    
    /// <summary>
    /// Factory Method لإنشاء Residence
    /// </summary>
    public static Residence Create(
        string name,
        Address address,
        int totalCapacity,
        Money monthlyRent,
        string? description = null,
        string? imagePath = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Residence name is required");
        
        if (totalCapacity <= 0)
            throw new ArgumentException("Total capacity must be greater than zero");
        
        var residence = new Residence
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = address,
            TotalCapacity = totalCapacity,
            AvailableCapacity = totalCapacity,
            MonthlyRent = monthlyRent,
            Description = description,
            ImagePath = imagePath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
        
        return residence;
    }
    
    /// <summary>
    /// إضافة غرفة للسكن
    /// </summary>
    public void AddRoom(Room room)
    {
        if (_rooms.Any(r => r.RoomNumber == room.RoomNumber))
            throw new InvalidOperationException($"Room {room.RoomNumber} already exists");
        
        _rooms.Add(room);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تخصيص Manager للسكن
    /// </summary>
    public void AssignManager(Guid managerId)
    {
        ManagerId = managerId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تحديث السعة المتاحة
    /// </summary>
    public void UpdateAvailableCapacity()
    {
        var occupiedBeds = _rooms.Sum(r => r.OccupiedBeds);
        AvailableCapacity = TotalCapacity - occupiedBeds;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تحديث المرافق
    /// </summary>
    public void UpdateFacilities(ResidenceFacilities facilities)
    {
        Facilities = facilities;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// تحديث معلومات السكن
    /// </summary>
    public void Update(string name, Address address, string? description, string? imagePath)
    {
        Name = name;
        Address = address;
        Description = description;
        ImagePath = imagePath;
        UpdatedAt = DateTime.UtcNow;
    }
}