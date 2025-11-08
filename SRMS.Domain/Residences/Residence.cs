using SRMS.Domain.Abstractions;
using SRMS.Domain.Common.ValueObjects;
using SRMS.Domain.Managers;
using SRMS.Domain.Rooms;

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
    public int TotalCapacity { get; private set; }
    public int AvailableCapacity { get; private set; }
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
    
}
