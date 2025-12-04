using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Rooms.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Rooms;

namespace SRMS.Application.Rooms.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IRepositories<Room> _roomRepository;

    public CreateRoomCommandHandler(
        IRepositories<Room> roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            ResidenceId = request.Room.ResidenceId,
            RoomNumber = request.Room.RoomNumber,
            Floor = request.Room.Floor,
            RoomType = request.Room.RoomType,
            TotalBeds = request.Room.TotalBeds,
            OccupiedBeds = 0,
            HasPrivateBathroom = request.Room.HasPrivateBathroom,
            HasAirConditioning = request.Room.HasAirConditioning,
            HasHeating = request.Room.HasHeating,
            HasWifi = request.Room.HasWifi,
            HasDesk = request.Room.HasDesk,
            HasWardrobe = request.Room.HasWardrobe,
            HasBalcony = request.Room.HasBalcony,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };

        var created = await _roomRepository.CreateAsync(room);

        return new RoomDto
        {
            Id = created.Id,
            RoomNumber = created.RoomNumber,
            Floor = created.Floor,
            RoomType = created.RoomType,
            TotalBeds = created.TotalBeds,
            OccupiedBeds = created.OccupiedBeds,
            AvailableBeds = created.TotalBeds - created.OccupiedBeds,
            IsFull = created.IsFull,
            ResidenceName = created.Residence?.Name ?? "",
            IsActive = created.IsActive
        };
    }
}
