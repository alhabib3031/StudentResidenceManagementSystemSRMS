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

    public UpdateRoomCommandHandler(
        IRepositories<Room> roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<RoomDto?> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var existing = await _roomRepository.GetByIdAsync(request.Room.Id);

        if (existing == null) return null;

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