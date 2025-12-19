using MediatR;
using SRMS.Application.Rooms.DTOs;

namespace SRMS.Application.Rooms.GetAllRooms;

public record GetAllRoomsQuery() : IRequest<List<RoomDto>>;
