using MediatR;
using SRMS.Application.Rooms.DTOs;

namespace SRMS.Application.Rooms.CreateRoom;

public class CreateRoomCommand : IRequest<RoomDto>
{
    public CreateRoomDto Room { get; set; } = new();
}