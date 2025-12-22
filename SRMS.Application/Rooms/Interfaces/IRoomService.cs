using SRMS.Application.Residences.DTOs;
using SRMS.Application.Rooms.DTOs;

namespace SRMS.Application.Rooms.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetRoomsByResidenceIdAsync(Guid residenceId); // Added for residence-specific room management
    Task<string?> GetResidenceNameAsync(Guid residenceId); // Added for displaying residence name in room management
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync(); // New method for admin
    Task<List<ResidenceDto>> GetAllResidencesAsync();
    Task<List<RoomDto>> GetAvailableRoomsByResidenceIdAsync(Guid residenceId);
    Task<RoomDetailsDto> GetRoomDetailsForStudentAsync(Guid roomId, Guid studentId);
    Task<RoomDto> CreateRoomAsync(CreateRoomDto roomDto);
    Task<RoomDto> UpdateRoomAsync(UpdateRoomDto roomDto);
    Task<bool> DeleteRoomAsync(Guid roomId); // Changed return type to Result<bool>
    Task<RoomDto> GetRoomByIdAsync(Guid roomId);
}