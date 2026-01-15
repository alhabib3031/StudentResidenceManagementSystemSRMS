using Moq;
using SRMS.Application.Students.DeleteStudent;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Application.AuditLogs.Interfaces;
using Xunit;
using FluentAssertions;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Testing.Application.Students.Commands;

public class DeleteStudentCommandHandlerTests
{
    private readonly Mock<IRepositories<Student>> _studentRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly DeleteStudentCommandHandler _handler;

    public DeleteStudentCommandHandlerTests()
    {
        _studentRepositoryMock = new Mock<IRepositories<Student>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new DeleteStudentCommandHandler(_studentRepositoryMock.Object, _auditServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnTrueAndLogDeletion_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student { Id = studentId, FirstName = "Test", LastName = "User" };
        var command = new DeleteStudentCommand { Id = studentId };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(r => r.DeleteAsync(studentId)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _studentRepositoryMock.Verify(r => r.GetByIdAsync(studentId), Times.Once);
        _studentRepositoryMock.Verify(r => r.DeleteAsync(studentId), Times.Once);
        _auditServiceMock.Verify(a => a.LogCrudAsync(
            AuditAction.Delete,
            It.IsAny<object>(),
            null,
            It.Is<string>(s => s.Contains("Student deleted (soft delete)"))
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFalseAndLogFailure_WhenStudentDoesNotExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var command = new DeleteStudentCommand { Id = studentId };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _studentRepositoryMock.Verify(r => r.GetByIdAsync(studentId), Times.Once);
        _studentRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _auditServiceMock.Verify(a => a.LogAsync(
            AuditAction.Failure,
            "Student",
            studentId.ToString(),
            null,
            null,
            "Attempted to delete non-existent student"
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFalseAndLogFailure_WhenRepositoryDeleteFails()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student { Id = studentId, FirstName = "Test", LastName = "User" };
        var command = new DeleteStudentCommand { Id = studentId };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(r => r.DeleteAsync(studentId)).ReturnsAsync(false); // Simulate repository failure

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _auditServiceMock.Verify(a => a.LogAsync(
            AuditAction.Failure,
            "Student",
            studentId.ToString(),
            null,
            null,
            It.Is<string>(s => s.Contains("Failed to delete student"))
        ), Times.Once);
    }
}
