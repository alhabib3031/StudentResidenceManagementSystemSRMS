using System.ComponentModel.DataAnnotations;
using SRMS.Domain.Rooms.Enums;

namespace SRMS.Application.Rooms.DTOs;

// ============================================================
// DTOs
// ============================================================
public class CreateRoomDto
{
    [Required]
    public Guid ResidenceId { get; set; }

    [Required]
    [StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [Range(0, 100)]
    public int Floor { get; set; }

    [Required]
    public RoomType RoomType { get; set; }

    [Required]
    [Range(1, 10)]
    public int TotalBeds { get; set; }

    // Amenities
    public bool HasPrivateBathroom { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool HasHeating { get; set; }
    public bool HasWifi { get; set; }
    public bool HasDesk { get; set; }
    public bool HasWardrobe { get; set; }
    public bool HasBalcony { get; set; }
}