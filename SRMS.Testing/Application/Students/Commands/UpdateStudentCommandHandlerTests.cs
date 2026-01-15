using Moq;
using SRMS.Application.Students.UpdateStudent;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Students.DTOs;
using Xunit;
using FluentAssertions;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.ValueObjects;
using SRMS.Domain.Students.Enums;

namespace SRMS.Testing.Application.Students.Commands;

public class UpdateStudentCommandHandlerTests
{
    private readonly Mock<IRepositories<Student>> _studentRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly UpdateStudentCommandHandler _handler;

    public UpdateStudentCommandHandlerTests()
    {
        _studentRepositoryMock = new Mock<IRepositories<Student>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new UpdateStudentCommandHandler(_studentRepositoryMock.Object, _auditServiceMock.Object);
    }

    private Student CreateSampleStudent()
    {
        return new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "OldFirst",
            LastName = "OldLast",
            Email = Email.Create("old@example.com"),
            Status = StudentStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };
    }

    [Fact]
    public async Task Handle_Should_UpdateStudentAndReturnDto_WhenRequestIsValid()
    {
        // Arrange
        var existingStudent = CreateSampleStudent();
        var updateDto = new UpdateStudentDto
        {
            Id = existingStudent.Id,
            FirstName = "NewFirst",
            LastName = "NewLast",
            Email = "new@example.com",
            Status = StudentStatus.Active,
            AcademicYear = 2
        };
        var command = new UpdateStudentCommand { Student = updateDto };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(existingStudent.Id)).ReturnsAsync(existingStudent);
        _studentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("NewFirst");
        result.Email.Should().Be("new@example.com");
        result.UpdatedAt.Should().BeAfter(existingStudent.CreatedAt);

        _studentRepositoryMock.Verify(r => r.GetByIdAsync(existingStudent.Id), Times.Once);
        _studentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Student>(s => s.FirstName == "NewFirst")), Times.Once);
        _auditServiceMock.Verify(a => a.LogCrudAsync(AuditAction.Update, It.IsAny<object>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenStudentDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateStudentCommand { Student = new UpdateStudentDto { Id = nonExistentId } };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((Student?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _studentRepositoryMock.Verify(r => r.GetByIdAsync(nonExistentId), Times.Once);
        _studentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Never);
        _auditServiceMock.Verify(a => a.LogAsync(AuditAction.Failure, "Student", nonExistentId.ToString(), null, null, "Attempted to update non-existent student"), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_LogStatusChange_WhenStatusIsDifferent()
    {
        // Arrange
        var existingStudent = CreateSampleStudent(); // Status is Active
        var updateDto = new UpdateStudentDto
        {
            Id = existingStudent.Id,
            FirstName = "NewFirst",
            LastName = "NewLast",
            Email = "new@example.com",
            Status = StudentStatus.Suspended, // Status changed
            AcademicYear = 2
        };
        var command = new UpdateStudentCommand { Student = updateDto };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(existingStudent.Id)).ReturnsAsync(existingStudent);
        _studentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _auditServiceMock.Verify(a => a.LogStudentStatusChangeAsync(
            existingStudent.Id,
            StudentStatus.Active.ToString(),
            StudentStatus.Suspended.ToString(),
            "Status changed via update"
        ), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenEmailIsInvalid()
    {
        // Arrange
        var existingStudent = CreateSampleStudent();
        var updateDto = new UpdateStudentDto
        {
            Id = existingStudent.Id,
            Email = "invalid-email"
        };
        var command = new UpdateStudentCommand { Student = updateDto };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(existingStudent.Id)).ReturnsAsync(existingStudent);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid email: Invalid email format");
        _studentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Never);
    }
}
