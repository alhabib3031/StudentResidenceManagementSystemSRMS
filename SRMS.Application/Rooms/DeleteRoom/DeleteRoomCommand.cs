using MediatR;

namespace SRMS.Application.Rooms.DeleteRoom;

public record DeleteRoomCommand(Guid RoomId) : IRequest<bool>;
