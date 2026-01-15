using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;
using Moq;
using SRMS.Domain.Rooms;
using SRMS.Domain.Residences;
using Microsoft.AspNetCore.Http;

namespace SRMS.Testing.Infrastructure.Repositories;

public class GenericRepositoryRoomTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly GenericRepository<Room> _repository;
    private readonly GenericRepository<Residence> _residenceRepository;

    public GenericRepositoryRoomTests()
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
        
        _repository = new GenericRepository<Room>(contextFactoryMock.Object);
        _residenceRepository = new GenericRepository<Residence>(contextFactoryMock.Object);
    }
    
    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    private async Task<Room> SeedRoomAsync()
    {
        var residence = await _residenceRepository.CreateAsync(new Residence { Name = "Test Residence" });
        var room = new Room { RoomNumber = "G01", Floor = 0, ResidenceId = residence.Id };
        return await _repository.CreateAsync(room);
    }

    [Fact]
    public async Task CreateAsync_Should_AddRoomToDatabase()
    {
        // Arrange
        var residence = await _residenceRepository.CreateAsync(new Residence { Name = "Test Residence" });
        var room = new Room { RoomNumber = "101", Floor = 1, ResidenceId = residence.Id };

        // Act
        var createdRoom = await _repository.CreateAsync(room);
        
        await using var context = CreateContext();
        var roomInDb = await context.Rooms.FindAsync(createdRoom.Id);

        // Assert
        roomInDb.Should().NotBeNull();
        roomInDb!.RoomNumber.Should().Be("101");
        createdRoom.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnCorrectRoom()
    {
        // Arrange
        var seededRoom = await SeedRoomAsync();

        // Act
        var foundRoom = await _repository.GetByIdAsync(seededRoom.Id);

        // Assert
        foundRoom.Should().NotBeNull();
        foundRoom!.Id.Should().Be(seededRoom.Id);
        foundRoom.RoomNumber.Should().Be("G01");
    }

    [Fact]
    public async Task DeleteAsync_Should_SoftDeleteRoom()
    {
        // Arrange
        var seededRoom = await SeedRoomAsync();

        // Act
        var result = await _repository.DeleteAsync(seededRoom.Id);
        var roomAfterDelete = await _repository.GetByIdAsync(seededRoom.Id);
        
        await using var context = CreateContext();
        var roomInDb = await context.Rooms.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == seededRoom.Id);

        // Assert
        result.Should().BeTrue();
        roomAfterDelete.Should().BeNull();
        roomInDb.Should().NotBeNull();
        roomInDb!.IsDeleted.Should().BeTrue();
    }
    
    [Fact]
    public async Task FindAsync_Should_ReturnRoomsMatchingPredicate()
    {
        // Arrange
        await SeedRoomAsync(); // Room G01
        var residence2 = await _residenceRepository.CreateAsync(new Residence { Name = "Second Residence" });
        await _repository.CreateAsync(new Room { RoomNumber = "S01", Floor = 0, ResidenceId = residence2.Id });

        // Act
        var roomsOnGroundFloor = await _repository.FindAsync(r => r.Floor == 0);

        // Assert
        roomsOnGroundFloor.Should().NotBeNull();
        roomsOnGroundFloor.Should().HaveCount(2);
    }
}
