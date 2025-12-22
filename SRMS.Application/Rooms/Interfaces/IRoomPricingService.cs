using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Rooms.Interfaces;

public interface IRoomPricingService
{
    Task<Money> CalculateRoomFee(Room room, Student student);
}
