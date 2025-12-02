using MediatR;
using SRMS.Application.Rooms.DTOs;

namespace SRMS.Application.Rooms.UpdateRoom;

public class UpdateRoomCommand : IRequest<RoomDto?>
{
    public UpdateRoomDto Room { get; set; } = new();
}