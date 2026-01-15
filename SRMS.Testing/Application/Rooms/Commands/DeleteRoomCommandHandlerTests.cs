using Moq;
using SRMS.Application.Rooms.DeleteRoom;
using SRMS.Domain.Repositories;
using SRMS.Domain.Rooms;
using SRMS.Domain.Residences;
using SRMS.Application.AuditLogs.Interfaces;
using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace SRMS.Testing.Application.Rooms.Commands;

public class DeleteRoomCommandHandlerTests
{
    private readonly Mock<IRepositories<Room>> _roomRepositoryMock;
    private readonly Mock<IRepositories<Residence>> _residenceRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly DeleteRoomCommandHandler _handler;

    public DeleteRoomCommandHandlerTests()
    {
        _roomRepositoryMock = new Mock<IRepositories<Room>>();
        _residenceRepositoryMock = new Mock<IRepositories<Residence>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new DeleteRoomCommandHandler(_roomRepositoryMock.Object, _residenceRepositoryMock.Object, _auditServiceMock.Object);
    }

    private Room CreateSampleRoom(Guid id, Guid residenceId, int totalBeds, int occupiedBeds)
    {
        return new Room
        {
            Id = id,
            ResidenceId = residenceId,
            RoomNumber = "101",
            TotalBeds = totalBeds,
            OccupiedBeds = occupiedBeds
        };
    }

    private Residence CreateSampleResidence(Guid id, int currentRooms, int totalCapacity, int availableCapacity)
    {
        return new Residence
        {
            Id = id,
            Name = "Test Residence",
            CurrentRoomsCount = currentRooms,
            TotalCapacity = totalCapacity,
            AvailableCapacity = availableCapacity
        };
    }

    [Fact]
    public async Task Handle_Should_DeleteRoomAndUpdateResidence_WhenRoomIsEmpty()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var residenceId = Guid.NewGuid();
        var room = CreateSampleRoom(roomId, residenceId, 4, 0); // Empty room
        var residence = CreateSampleResidence(residenceId, 5, 20, 10);
        var command = new DeleteRoomCommand(roomId);

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _residenceRepositoryMock.Setup(r => r.GetByIdAsync(residenceId)).ReturnsAsync(residence);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        // Verify room is soft-deleted
        _roomRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Room>(
            r => r.Id == roomId && r.IsDeleted == true && r.IsActive == false
        )), Times.Once);

        // Verify residence is updated
        _residenceRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Residence>(res =>
            res.Id == residenceId &&
            res.CurrentRoomsCount == 4 && // 5 - 1
            res.TotalCapacity == 16 &&   // 20 - 4
            res.AvailableCapacity == 6   // 10 - 4
        )), Times.Once);

        _auditServiceMock.Verify(a => a.LogCrudAsync(SRMS.Domain.AuditLogs.Enums.AuditAction.Delete, It.IsAny<object>(), null, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFalse_WhenRoomNotFound()
    {
        // Arrange
        var command = new DeleteRoomCommand(Guid.NewGuid());
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Room?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _roomRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Never);
        _residenceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Residence>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_WhenRoomIsOccupied()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var residenceId = Guid.NewGuid();
        var room = CreateSampleRoom(roomId, residenceId, 4, 1); // Occupied room
        var command = new DeleteRoomCommand(roomId);

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Cannot delete a room that has occupied beds. Please move or remove students first.");
        _roomRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Never);
        _residenceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Residence>()), Times.Never);
    }
}
