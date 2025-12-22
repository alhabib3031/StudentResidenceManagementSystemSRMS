using MediatR;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.Rooms.DTOs;
using SRMS.Domain.Rooms;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Rooms.GetAllRooms;

public class GetAllRoomsQueryHandler : IRequestHandler<GetAllRoomsQuery, List<RoomDto>>
{
    private readonly IRepositories<Room> _roomRepo;

    public GetAllRoomsQueryHandler(IRepositories<Room> roomRepo)
    {
        _roomRepo = roomRepo;
    }

    public async Task<List<RoomDto>> Handle(GetAllRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _roomRepo.Query()
            .Where(r => !r.IsDeleted)
            .Include(r => r.Residence)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                RoomType = r.RoomType,
                TotalBeds = r.TotalBeds,
                OccupiedBeds = r.OccupiedBeds,
                IsFull = r.OccupiedBeds >= r.TotalBeds,
                ResidenceName = r.Residence.Name,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);

        return rooms;
    }
}
