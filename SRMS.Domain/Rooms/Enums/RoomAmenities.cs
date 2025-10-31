namespace SRMS.Domain.Rooms.Enums;

public record RoomAmenities
{
    public bool HasPrivateBathroom { get; init; }
    public bool HasAirConditioning { get; init; }
    public bool HasHeating { get; init; }
    public bool HasWifi { get; init; }
    public bool HasDesk { get; init; }
    public bool HasWardrobe { get; init; }
    public bool HasBalcony { get; init; }
}