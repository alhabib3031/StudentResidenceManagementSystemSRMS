using MapsterMapper;
using SRMS.Application.Residences.DTOs;
using SRMS.Application.Rooms.DTOs;
using SRMS.Application.Rooms.Interfaces;
using SRMS.Domain.Repositories;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms;
using SRMS.Domain.Rooms.Enums;
using SRMS.Domain.Students;
using SRMS.Domain.Students.Enums;
using SRMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SRMS.Infrastructure.Configurations.Services;

public class RoomService : IRoomService
{
    private readonly IRepositories<Residence> _residenceRepository;
    private readonly IRepositories<Room> _roomRepository;
    private readonly IRepositories<Student> _studentRepository;
    private readonly IMapper _mapper;

    public RoomService(
        IRepositories<Residence> residenceRepository,
        IRepositories<Room> roomRepository,
        IRepositories<Student> studentRepository,
        IMapper mapper)
    {
        _residenceRepository = residenceRepository;
        _roomRepository = roomRepository;
        _studentRepository = studentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsByResidenceIdAsync(Guid residenceId)
    {
        var residence = await _residenceRepository.GetByIdAsync(residenceId);
        if (residence == null)
        {
            return new List<RoomDto>(); // Return an empty list if residence not found
        }

        var rooms = await _roomRepository.Query()
            .Where(r => r.ResidenceId == residenceId)
            .Include(r => r.Residence)
            .ToListAsync();

        var roomDtos = rooms.Select(r =>
        {
            var roomDto = _mapper.Map<RoomDto>(r);
            roomDto.MonthlyRentAmount = r.MonthlyRent?.Amount;
            roomDto.MonthlyRentCurrency = r.MonthlyRent?.Currency;
            roomDto.ResidenceName = r.Residence?.Name ?? "N/A";
            return roomDto;
        }).ToList();

        return roomDtos;
    }

    public async Task<string?> GetResidenceNameAsync(Guid residenceId)
    {
        var residence = await _residenceRepository.GetByIdAsync(residenceId);
        return residence?.Name;
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
    {
        // Eager load Residence for mapping ResidenceName
        var rooms = await _roomRepository.Query().Include(r => r.Residence).ToListAsync();
        var roomDtos = rooms.Select(r =>
        {
            var roomDto = _mapper.Map<RoomDto>(r);
            roomDto.MonthlyRentAmount = r.MonthlyRent?.Amount;
            roomDto.MonthlyRentCurrency = r.MonthlyRent?.Currency;
            roomDto.ResidenceName = r.Residence?.Name ?? "N/A";
            return roomDto;
        }).ToList();
        return roomDtos;
    }

    public async Task<List<ResidenceDto>> GetAllResidencesAsync()
    {
        var residences = await _residenceRepository.GetAllAsync();
        return _mapper.Map<List<ResidenceDto>>(residences);
    }

    public async Task<List<RoomDto>> GetAvailableRoomsByResidenceIdAsync(Guid residenceId)
    {
        var rooms = await _roomRepository.FindAsync(r => r.ResidenceId == residenceId && !r.IsFull);
        var roomDtos = _mapper.Map<List<RoomDto>>(rooms);

        // Fetch residence names to populate ResidenceName in RoomDto
        var residence = await _residenceRepository.GetByIdAsync(residenceId);
        if (residence != null)
        {
            foreach (var roomDto in roomDtos)
            {
                roomDto.ResidenceName = residence.Name;
            }
        }

        return roomDtos;
    }

    public async Task<RoomDetailsDto> GetRoomDetailsForStudentAsync(Guid roomId, Guid studentId)
    {
        var room = await _roomRepository.Query()
            .Include(r => r.Reservations)
            .ThenInclude(res => res.Student)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        var student = await _studentRepository.GetByIdAsync(studentId);

        if (room == null || student == null)
        {
            throw new Exception("Room or Student not found.");
        }

        var residence = await _residenceRepository.GetByIdAsync(room.ResidenceId);
        if (residence == null)
        {
            throw new Exception("Residence not found for the room.");
        }

        var roomDetailsDto = _mapper.Map<RoomDetailsDto>(room);
        roomDetailsDto.ResidenceName = residence.Name;
        roomDetailsDto.BaseMonthlyRent = room.MonthlyRent?.Amount ?? 0;

        // Calculate adjusted price based on student rank/nationality
        decimal adjustedPrice = room.MonthlyRent?.Amount ?? 0;
        if (student.StudyLevel == StudyLevel.Master) adjustedPrice *= 0.95m;
        else if (student.StudyLevel == StudyLevel.PhD) adjustedPrice *= 0.90m;

        if (student.NationalityId.HasValue && student.Nationality?.Name != "Libyan")
        {
            adjustedPrice *= 1.10m;
        }

        roomDetailsDto.AdjustedMonthlyRent = adjustedPrice;
        roomDetailsDto.StudentStudyLevel = student.StudyLevel.ToString();
        roomDetailsDto.StudentNationality = student.Nationality?.Name ?? "N/A";

        // Populate Roommates (Active residents excluding current student)
        roomDetailsDto.Roommates = room.Reservations
            .Where(r => r.Status == Domain.Reservations.Enums.ReservationStatus.Confirmed
                     && r.StudentId != studentId
                     && r.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow)) // Ensure they are currently there
            .Select(r => r.Student.FullName)
            .ToList();

        return roomDetailsDto;
    }

    public async Task<RoomDto> GetRoomByIdAsync(Guid roomId)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            return null; // Or throw a specific exception if preferred
        }

        var roomDto = _mapper.Map<RoomDto>(room);
        roomDto.MonthlyRentAmount = room.MonthlyRent?.Amount;
        roomDto.MonthlyRentCurrency = room.MonthlyRent?.Currency;

        // Fetch residence name
        var residence = await _residenceRepository.GetByIdAsync(room.ResidenceId);
        roomDto.ResidenceName = residence?.Name ?? "N/A";

        return roomDto;
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto roomDto)
    {
        // Check if Residence exists
        var residence = await _residenceRepository.GetByIdAsync(roomDto.ResidenceId);
        if (residence == null)
        {
            throw new Exception($"Residence with ID {roomDto.ResidenceId} not found.");
        }

        var room = new Room
        {
            RoomNumber = roomDto.RoomNumber,
            Floor = roomDto.Floor,
            RoomType = roomDto.RoomType,
            TotalBeds = roomDto.TotalBeds,
            OccupiedBeds = 0, // New rooms start with 0 occupied beds
            MonthlyRent = Money.Create(roomDto.MonthlyRentAmount, roomDto.MonthlyRentCurrency),
            ResidenceId = roomDto.ResidenceId,
            HasPrivateBathroom = roomDto.HasPrivateBathroom,
            HasAirConditioning = roomDto.HasAirConditioning,
            HasHeating = roomDto.HasHeating,
            HasWifi = roomDto.HasWifi,
            HasDesk = roomDto.HasDesk,
            HasWardrobe = roomDto.HasWardrobe,
            HasBalcony = roomDto.HasBalcony,
            Status = RoomStatus.Available // New rooms are available by default
        };

        var createdRoom = await _roomRepository.CreateAsync(room);
        await _roomRepository.SaveChangesAsync(); // Persist changes

        // Update residence's room count and capacity
        residence.CurrentRoomsCount++;
        // Assuming each room adds its total beds to the residence's capacity
        residence.TotalCapacity += room.TotalBeds;
        residence.AvailableCapacity += room.TotalBeds;
        await _residenceRepository.UpdateAsync(residence);
        await _residenceRepository.SaveChangesAsync();


        var roomToReturn = _mapper.Map<RoomDto>(createdRoom);
        roomToReturn.ResidenceName = residence.Name;
        return roomToReturn;
    }

    public async Task<RoomDto> UpdateRoomAsync(UpdateRoomDto roomDto)
    {
        var existingRoom = await _roomRepository.GetByIdAsync(roomDto.Id);
        if (existingRoom == null)
        {
            throw new Exception($"Room with ID {roomDto.Id} not found.");
        }

        var oldTotalBeds = existingRoom.TotalBeds;

        // Update properties
        existingRoom.RoomNumber = roomDto.RoomNumber;
        existingRoom.Floor = roomDto.Floor;
        existingRoom.RoomType = roomDto.RoomType;
        existingRoom.TotalBeds = roomDto.TotalBeds;
        existingRoom.OccupiedBeds = roomDto.OccupiedBeds;
        existingRoom.MonthlyRent = Money.Create(roomDto.MonthlyRentAmount, roomDto.MonthlyRentCurrency);
        existingRoom.ResidenceId = roomDto.ResidenceId; // Allow changing residence if needed
        existingRoom.HasPrivateBathroom = roomDto.HasPrivateBathroom;
        existingRoom.HasAirConditioning = roomDto.HasAirConditioning;
        existingRoom.HasHeating = roomDto.HasHeating;
        existingRoom.HasWifi = roomDto.HasWifi;
        existingRoom.HasDesk = roomDto.HasDesk;
        existingRoom.HasWardrobe = roomDto.HasWardrobe;
        existingRoom.HasBalcony = roomDto.HasBalcony;

        var updatedRoom = await _roomRepository.UpdateAsync(existingRoom);
        await _roomRepository.SaveChangesAsync();

        // Update residence capacity if total beds changed
        if (oldTotalBeds != roomDto.TotalBeds)
        {
            var residence = await _residenceRepository.GetByIdAsync(roomDto.ResidenceId);
            if (residence != null)
            {
                residence.TotalCapacity = residence.TotalCapacity - oldTotalBeds + roomDto.TotalBeds;
                residence.AvailableCapacity = residence.AvailableCapacity - (oldTotalBeds - existingRoom.OccupiedBeds) + (roomDto.TotalBeds - existingRoom.OccupiedBeds);
                await _residenceRepository.UpdateAsync(residence);
                await _residenceRepository.SaveChangesAsync();
            }
        }

        var roomToReturn = _mapper.Map<RoomDto>(updatedRoom);
        var residenceName = (await _residenceRepository.GetByIdAsync(updatedRoom.ResidenceId))?.Name;
        if (residenceName != null) roomToReturn.ResidenceName = residenceName;
        return roomToReturn;
    }

    public async Task<bool> DeleteRoomAsync(Guid roomId)
    {
        var existingRoom = await _roomRepository.GetByIdAsync(roomId);
        if (existingRoom == null)
        {
            return false; // Room not found
        }

        if (existingRoom.OccupiedBeds > 0)
        {
            throw new Exception("Cannot delete a room that has occupied beds.");
        }

        var residence = await _residenceRepository.GetByIdAsync(existingRoom.ResidenceId);

        var deleted = await _roomRepository.DeleteAsync(roomId);
        await _roomRepository.SaveChangesAsync();

        if (deleted && residence != null)
        {
            // Update residence's room count and capacity
            residence.CurrentRoomsCount--;
            residence.TotalCapacity -= existingRoom.TotalBeds;
            residence.AvailableCapacity -= (existingRoom.TotalBeds - existingRoom.OccupiedBeds);
            await _residenceRepository.UpdateAsync(residence);
            await _residenceRepository.SaveChangesAsync();
        }

        if (!deleted)
        {
            return false; // Failed to delete room in the database.
        }

        return true;
    }
}