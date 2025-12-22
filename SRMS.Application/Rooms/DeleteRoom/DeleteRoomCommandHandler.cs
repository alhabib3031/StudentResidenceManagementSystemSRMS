using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms;
using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Rooms.DeleteRoom;

public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IRepositories<Room> _roomRepository;
    private readonly IRepositories<Residence> _residenceRepository;
    private readonly IAuditService _audit;

    public DeleteRoomCommandHandler(
        IRepositories<Room> roomRepository,
        IRepositories<Residence> residenceRepository,
        IAuditService audit)
    {
        _roomRepository = roomRepository;
        _residenceRepository = residenceRepository;
        _audit = audit;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        if (room == null) return false;

        if (room.OccupiedBeds > 0)
        {
            throw new ValidationException("Cannot delete a room that has occupied beds. Please move or remove students first.");
        }

        var residence = await _residenceRepository.GetByIdAsync(room.ResidenceId);
        if (residence != null)
        {
            residence.CurrentRoomsCount--;
            // Since occupied beds must be 0, TotalBeds == AvailableBeds for this room
            residence.TotalCapacity -= room.TotalBeds;
            residence.AvailableCapacity -= room.TotalBeds;

            // Safety checks
            if (residence.CurrentRoomsCount < 0) residence.CurrentRoomsCount = 0;
            if (residence.TotalCapacity < 0) residence.TotalCapacity = 0;
            if (residence.AvailableCapacity < 0) residence.AvailableCapacity = 0;

            await _residenceRepository.UpdateAsync(residence);
        }

        room.IsDeleted = true;
        room.IsActive = false;
        await _roomRepository.UpdateAsync(room);

        await _audit.LogCrudAsync(
            action: AuditAction.Delete,
            oldEntity: new { room.Id, room.RoomNumber, room.ResidenceId },
            additionalInfo: $"Room Deleted: {room.RoomNumber}"
        );

        return true;
    }
}
