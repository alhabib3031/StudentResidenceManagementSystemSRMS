using Moq;
using SRMS.Application.Rooms.CreateRoom;
using SRMS.Domain.Repositories;
using SRMS.Domain.Rooms;
using SRMS.Domain.Residences;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Rooms.DTOs;
using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using MockQueryable.Moq;

namespace SRMS.Testing.Application.Rooms.Commands;

public class CreateRoomCommandHandlerTests
{
    private readonly Mock<IRepositories<Room>> _roomRepositoryMock;
    private readonly Mock<IRepositories<Residence>> _residenceRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly CreateRoomCommandHandler _handler;

    public CreateRoomCommandHandlerTests()
    {
        _roomRepositoryMock = new Mock<IRepositories<Room>>();
        _residenceRepositoryMock = new Mock<IRepositories<Residence>>();
        _auditServiceMock = new Mock<IAuditService>();

        _handler = new CreateRoomCommandHandler(
            _roomRepositoryMock.Object,
            _residenceRepositoryMock.Object,
            _auditServiceMock.Object
        );
    }

    private Residence CreateSampleResidence(Guid id, int currentRooms = 0, int maxRooms = 10)
    {
        return new Residence
        {
            Id = id,
            Name = "Test Residence",
            CurrentRoomsCount = currentRooms,
            MaxRoomsCount = maxRooms
        };
    }

    [Fact]
    public async Task Handle_Should_CreateRoom_WhenRequestIsValid()
    {
        // Arrange
        var residenceId = Guid.NewGuid();
        var residence = CreateSampleResidence(residenceId);

        var createRoomDto = new CreateRoomDto
        {
            ResidenceId = residenceId,
            RoomNumber = "101",
            TotalBeds = 2
        };

        var command = new CreateRoomCommand { Room = createRoomDto };

        IQueryable<Room> mockRooms = new List<Room>()
            .AsQueryable()
            .BuildMock();

        _residenceRepositoryMock
            .Setup(r => r.GetByIdAsync(residenceId))
            .ReturnsAsync(residence);

        _roomRepositoryMock
            .Setup(r => r.Query())
            .Returns(mockRooms);

        _roomRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room r) => r);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoomNumber.Should().Be("101");

        _roomRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Room>(room => room.RoomNumber == "101")),
            Times.Once
        );

        _residenceRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Residence>(
                res => res.CurrentRoomsCount == 1 && res.TotalCapacity == 2
            )),
            Times.Once
        );

        _auditServiceMock.Verify(
            a => a.LogCrudAsync(
                SRMS.Domain.AuditLogs.Enums.AuditAction.Create,
                null,
                It.IsAny<object>(),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_Should_ThrowException_WhenResidenceNotFound()
    {
        // Arrange
        var command = new CreateRoomCommand
        {
            Room = new CreateRoomDto { ResidenceId = Guid.NewGuid() }
        };

        _residenceRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Residence?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage($"Residence with ID {command.Room.ResidenceId} not found");
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_WhenResidenceIsAtMaxCapacity()
    {
        // Arrange
        var residenceId = Guid.NewGuid();
        var residence = CreateSampleResidence(residenceId, currentRooms: 10, maxRooms: 10);

        var command = new CreateRoomCommand
        {
            Room = new CreateRoomDto { ResidenceId = residenceId }
        };

        _residenceRepositoryMock
            .Setup(r => r.GetByIdAsync(residenceId))
            .ReturnsAsync(residence);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage(
                $"Residence {residence.Name} has reached its maximum room capacity of {residence.MaxRoomsCount}."
            );
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_WhenRoomNumberIsDuplicate()
    {
        // Arrange
        var residenceId = Guid.NewGuid();
        var residence = CreateSampleResidence(residenceId);

        var command = new CreateRoomCommand
        {
            Room = new CreateRoomDto
            {
                ResidenceId = residenceId,
                RoomNumber = "101"
            }
        };

        var existingRoom = new Room
        {
            RoomNumber = "101",
            ResidenceId = residenceId
        };

        IQueryable<Room> mockRooms = new List<Room> { existingRoom }
            .AsQueryable()
            .BuildMock();

        _residenceRepositoryMock
            .Setup(r => r.GetByIdAsync(residenceId))
            .ReturnsAsync(residence);

        _roomRepositoryMock
            .Setup(r => r.Query())
            .Returns(mockRooms);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage(
                $"Room number {command.Room.RoomNumber} already exists in residence {residence.Name}."
            );
    }
}
