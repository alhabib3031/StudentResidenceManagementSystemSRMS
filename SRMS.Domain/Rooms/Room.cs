using SRMS.Domain.Abstractions;
using SRMS.Domain.Rooms.Enums;
using SRMS.Domain.Students;

namespace SRMS.Domain.Rooms;

/// <summary>
/// Room (الغرفة) - Entity
/// </summary>
public class Room : Entity
{
    public string RoomNumber { get; private set; } = string.Empty;
    public int Floor { get; private set; }
    public RoomType RoomType { get; private set; }
    public RoomAmenities Amenities { get; private set; } = new();
    public int TotalBeds { get; private set; }
    public int OccupiedBeds { get; private set; }
    public bool IsFull => OccupiedBeds >= TotalBeds;

    // Navigation Properties
    public Guid ResidenceId { get; private set; }
    public Residences.Residence Residence { get; private set; } = null!;

    private readonly List<Student> _students = new();
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();
    private Room() { }

    public static Room Create(
        string roomNumber, 
        int floor, 
        RoomType roomType,
        int totalBeds, 
        RoomAmenities? amenities = null)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number is required");

        if (totalBeds <= 0)
            throw new ArgumentException("Total beds must be greater than zero");
            
        if (floor <= 0)
            throw new ArgumentException("Floor must be greater than zero");

        return new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = roomNumber,
            Floor = floor,
            RoomType = roomType,
            TotalBeds = totalBeds,
            OccupiedBeds = 0,
            Amenities = amenities ?? new RoomAmenities(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
    }

    public void UpdateOccupancy(int occupiedBeds)
    {
        if (occupiedBeds < 0 || occupiedBeds > TotalBeds)
            throw new ArgumentException("Invalid number of occupied beds");

        OccupiedBeds = occupiedBeds;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateAmenities(RoomAmenities amenities)
    {
        Amenities = amenities;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddStudent(Student student)
    {
        if (IsFull)
            throw new InvalidOperationException("Room is full");
            
        if (_students.Any(s => s.Id == student.Id))
            throw new InvalidOperationException("Student already assigned to this room");
            
        _students.Add(student);
        OccupiedBeds++;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveStudent(Student student)
    {
        if (!_students.Any(s => s.Id == student.Id))
            throw new InvalidOperationException("Student not found in this room");
            
        _students.Remove(student);
        OccupiedBeds--;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateRoom(string roomNumber, int floor, RoomType roomType, int totalBeds)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number is required");
            
        if (totalBeds < OccupiedBeds)
            throw new ArgumentException("Total beds cannot be less than occupied beds");
            
        if (floor <= 0)
            throw new ArgumentException("Floor must be greater than zero");
            
        RoomNumber = roomNumber;
        Floor = floor;
        RoomType = roomType;
        TotalBeds = totalBeds;
        UpdatedAt = DateTime.UtcNow;
    }
}