using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Repositories;
using SRMS.Infrastructure.Configurations.Services;
using Xunit;
using FluentAssertions;
using Moq;
using Mapster;
using MapsterMapper;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Application.Rooms.DTOs;
using SRMS.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace SRMS.Testing.Infrastructure.Services;

public class RoomServiceTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly RoomService _roomService;
    private readonly GenericRepository<Residence> _residenceRepository;
    private readonly GenericRepository<Room> _roomRepository;

    public RoomServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

        var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
        contextFactoryMock.Setup(f => f.CreateDbContext())
            .Returns(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));
        contextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));

        _residenceRepository = new GenericRepository<Residence>(contextFactoryMock.Object);
        _roomRepository = new GenericRepository<Room>(contextFactoryMock.Object);
        var studentRepository = new GenericRepository<Student>(contextFactoryMock.Object);

        var config = new TypeAdapterConfig();
        config.NewConfig<Room, RoomDto>();
        var mapper = new Mapper(config);

        _roomService = new RoomService(_residenceRepository, _roomRepository, studentRepository, mapper);
    }

    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    private async Task<Residence> SeedResidenceAsync(string name = "Test Residence")
    {
        var residence = new Residence { Name = name, TotalCapacity = 0, CurrentRoomsCount = 0 };
        return await _residenceRepository.CreateAsync(residence);
    }

    private async Task<Room> SeedRoomAsync(Guid residenceId, string roomNumber = "101")
    {
        var room = new Room { ResidenceId = residenceId, RoomNumber = roomNumber, TotalBeds = 2, MonthlyRent = Money.Create(500, "USD") };
        return await _roomRepository.CreateAsync(room);
    }

    [Fact]
    public async Task CreateRoomAsync_Should_AddRoom_And_UpdateResidenceCapacity()
    {
        // Arrange
        var residence = await SeedResidenceAsync();
        var createRoomDto = new CreateRoomDto
        {
            ResidenceId = residence.Id,
            RoomNumber = "101",
            TotalBeds = 4,
            MonthlyRentAmount = 500
        };

        // Act
        var result = await _roomService.CreateRoomAsync(createRoomDto);
        
        using var context = CreateContext();
        var residenceInDb = await context.Residences.FindAsync(residence.Id);
        var roomInDb = await context.Rooms.FindAsync(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.RoomNumber.Should().Be("101");
        
        roomInDb.Should().NotBeNull();
        roomInDb!.TotalBeds.Should().Be(4);

        residenceInDb.Should().NotBeNull();
        residenceInDb!.CurrentRoomsCount.Should().Be(1);
        residenceInDb.TotalCapacity.Should().Be(4);
        residenceInDb.AvailableCapacity.Should().Be(4);
    }

    [Fact]
    public async Task CreateRoomAsync_Should_ThrowException_WhenResidenceNotFound()
    {
        // Arrange
        var createRoomDto = new CreateRoomDto { ResidenceId = Guid.NewGuid() };

        // Act
        Func<Task> act = async () => await _roomService.CreateRoomAsync(createRoomDto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage($"Residence with ID {createRoomDto.ResidenceId} not found.");
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_RemoveRoom_And_UpdateResidenceCapacity()
    {
        // Arrange
        var residence = await SeedResidenceAsync();
        var room = await _roomService.CreateRoomAsync(new CreateRoomDto { ResidenceId = residence.Id, RoomNumber = "101", TotalBeds = 2 });
        
        // Act
        var result = await _roomService.DeleteRoomAsync(room.Id);
        
        using var context = CreateContext();
        var residenceInDb = await context.Residences.FindAsync(residence.Id);
        var roomInDb = await context.Rooms.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == room.Id);

        // Assert
        result.Should().BeTrue();
        roomInDb.Should().NotBeNull();
        roomInDb!.IsDeleted.Should().BeTrue();
        
        residenceInDb.Should().NotBeNull();
        residenceInDb!.CurrentRoomsCount.Should().Be(0);
        residenceInDb.TotalCapacity.Should().Be(0);
        residenceInDb.AvailableCapacity.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRoomAsync_Should_ThrowException_WhenRoomIsOccupied()
    {
        // Arrange
        var residence = await SeedResidenceAsync();
        var roomDto = await _roomService.CreateRoomAsync(new CreateRoomDto { ResidenceId = residence.Id, RoomNumber = "101", TotalBeds = 2 });
        
        using (var context = CreateContext())
        {
            var room = await context.Rooms.FindAsync(roomDto.Id);
            room!.OccupiedBeds = 1;
            await context.SaveChangesAsync();
        }
        
        // Act
        Func<Task> act = async () => await _roomService.DeleteRoomAsync(roomDto.Id);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Cannot delete a room that has occupied beds.");
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_ReturnRoomDtoWithResidenceName_WhenRoomExists()
    {
        // Arrange
        var residence = await SeedResidenceAsync("Main Residence");
        var room = await SeedRoomAsync(residence.Id, "G01");

        // Act
        var result = await _roomService.GetRoomByIdAsync(room.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(room.Id);
        result.RoomNumber.Should().Be("G01");
        result.ResidenceName.Should().Be("Main Residence");
        result.MonthlyRentAmount.Should().Be(500);
    }

    [Fact]
    public async Task GetRoomByIdAsync_Should_ReturnNull_WhenRoomDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _roomService.GetRoomByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        // No need to dispose options or mock
    }
}
