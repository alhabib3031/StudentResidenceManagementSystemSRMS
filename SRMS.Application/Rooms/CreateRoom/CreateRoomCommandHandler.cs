using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Rooms.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Rooms;
using SRMS.Domain.Residences; // Added
using SRMS.Application.Common.Exceptions; // Added
using System.ComponentModel.DataAnnotations; // Added

namespace SRMS.Application.Rooms.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IRepositories<Room> _roomRepository;
    private readonly IRepositories<Residence> _residenceRepository;
    private readonly IAuditService _audit;

    public CreateRoomCommandHandler(
        IRepositories<Room> roomRepository,
        IRepositories<Residence> residenceRepository,
        IAuditService audit)
    {
        _roomRepository = roomRepository;
        _residenceRepository = residenceRepository;
        _audit = audit;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var residence = await _residenceRepository.GetByIdAsync(request.Room.ResidenceId);
        if (residence == null)
        {
            throw new NotFoundException(nameof(Residence), request.Room.ResidenceId);
        }

        // 1. Check MaxRoomsCount
        if (residence.CurrentRoomsCount >= residence.MaxRoomsCount)
        {
            throw new ValidationException($"Residence {residence.Name} has reached its maximum room capacity of {residence.MaxRoomsCount}.");
        }

        // 2. Check for duplicate room number in the same residence
        var existingRoom = await _roomRepository.GetSingleAsync(r => r.ResidenceId == request.Room.ResidenceId && r.RoomNumber == request.Room.RoomNumber);
        if (existingRoom != null)
        {
            throw new ValidationException($"Room number {request.Room.RoomNumber} already exists in residence {residence.Name}.");
        }

        // 3. Create the room
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

        // 4. Update Residence's CurrentRoomsCount and TotalCapacity
        residence.CurrentRoomsCount++;
        residence.TotalCapacity += created.TotalBeds;
        residence.AvailableCapacity += created.TotalBeds;
        await _residenceRepository.UpdateAsync(residence);

        // ✅ Log room creation
        await _audit.LogCrudAsync(
            action: AuditAction.Create,
            newEntity: new
            {
                created.Id,
                created.RoomNumber,
                created.Floor,
                created.RoomType,
                created.TotalBeds,
                created.ResidenceId
            },
            additionalInfo: $"New room created: {created.RoomNumber} on Floor {created.Floor}"
        );

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
