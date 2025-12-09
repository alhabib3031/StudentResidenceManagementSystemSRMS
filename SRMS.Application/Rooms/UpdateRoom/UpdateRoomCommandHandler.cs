using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Rooms.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Rooms;

namespace SRMS.Application.Rooms.UpdateRoom;

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, RoomDto?>
{
    private readonly IRepositories<Room> _roomRepository;
    private readonly IAuditService _audit;

    public UpdateRoomCommandHandler(
        IRepositories<Room> roomRepository,
        IAuditService audit)
    {
        _roomRepository = roomRepository;
        _audit = audit;
    }

    public async Task<RoomDto?> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var existing = await _roomRepository.GetByIdAsync(request.Room.Id);

        if (existing == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Room",
                request.Room.Id.ToString(),
                additionalInfo: "Attempted to update non-existent room"
            );
            return null;
        }

        var oldValues = new
        {
            existing.RoomNumber,
            existing.Floor,
            existing.RoomType,
            existing.TotalBeds,
            existing.OccupiedBeds
        };

        existing.RoomNumber = request.Room.RoomNumber;
        existing.Floor = request.Room.Floor;
        existing.RoomType = request.Room.RoomType;
        existing.TotalBeds = request.Room.TotalBeds;
        existing.OccupiedBeds = request.Room.OccupiedBeds;
        existing.HasPrivateBathroom = request.Room.HasPrivateBathroom;
        existing.HasAirConditioning = request.Room.HasAirConditioning;
        existing.HasHeating = request.Room.HasHeating;
        existing.HasWifi = request.Room.HasWifi;
        existing.HasDesk = request.Room.HasDesk;
        existing.HasWardrobe = request.Room.HasWardrobe;
        existing.HasBalcony = request.Room.HasBalcony;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _roomRepository.UpdateAsync(existing);

        var newValues = new
        {
            updated.RoomNumber,
            updated.Floor,
            updated.RoomType,
            updated.TotalBeds,
            updated.OccupiedBeds
        };

        await _audit.LogCrudAsync(
            action: AuditAction.Update,
            oldEntity: oldValues,
            newEntity: newValues,
            additionalInfo: $"Room updated: {updated.RoomNumber}"
        );

        return new RoomDto
        {
            Id = updated.Id,
            RoomNumber = updated.RoomNumber,
            Floor = updated.Floor,
            RoomType = updated.RoomType,
            TotalBeds = updated.TotalBeds,
            OccupiedBeds = updated.OccupiedBeds,
            AvailableBeds = updated.TotalBeds - updated.OccupiedBeds,
            IsFull = updated.IsFull,
            ResidenceName = updated.Residence?.Name ?? "",
            IsActive = updated.IsActive
        };
    }
}