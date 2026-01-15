using Moq;
using SRMS.Application.Students.CreateStudent;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.ValueObjects;
using Xunit;
using FluentAssertions;
using SRMS.Domain.AuditLogs.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update; // FIX: Added correct using for DbUpdateException

namespace SRMS.Testing.Application.Students.Commands;

public class CreateStudentCommandHandlerTests
{
    private readonly Mock<IRepositories<Student>> _studentRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly CreateStudentCommandHandler _handler;

    public CreateStudentCommandHandlerTests()
    {
        _studentRepositoryMock = new Mock<IRepositories<Student>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new CreateStudentCommandHandler(_studentRepositoryMock.Object, _auditServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateStudentAndReturnDto_WhenRequestIsValid()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "912345678",
            PhoneCountryCode = "+218",
            AcademicYear = 1
        };
        var command = new CreateStudentCommand { Student = createStudentDto };

        _studentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => 
            {
                s.Id = Guid.NewGuid(); // Simulate database generating an ID
                return s;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(createStudentDto.FirstName);
        result.LastName.Should().Be(createStudentDto.LastName);
        result.Email.Should().Be(createStudentDto.Email);
        result.PhoneNumber.Should().Be("+218 912345678");
        result.Id.Should().NotBe(Guid.Empty);

        // Verify that the repository's CreateAsync method was called exactly once
        _studentRepositoryMock.Verify(r => r.CreateAsync(It.Is<Student>(s => 
            s.FirstName == "John" &&
            s.Email!.Value == "john.doe@example.com"
        )), Times.Once);

        // Verify that the audit log was called for student registration
        _auditServiceMock.Verify(a => a.LogCrudAsync(
            AuditAction.StudentRegistered,
            null, // oldEntity
            It.Is<object>(o => o.GetType().GetProperty("FullName")!.GetValue(o)!.ToString() == "John Doe"), // newEntity
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new CreateStudentCommand 
        { 
            Student = new CreateStudentDto { Email = "invalid-email", AcademicYear = 1 } 
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid email: Invalid email format");
        
        // Verify that an error audit log was created
        _auditServiceMock.Verify(a => a.LogAsync(
            AuditAction.Error,
            "Student",
            null, null, null,
            "Invalid email during student creation: Invalid email format"), Times.Once);
        
        // Verify that CreateAsync was never called
        _studentRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var command = new CreateStudentCommand 
        { 
            Student = new CreateStudentDto 
            { 
                Email = "test@example.com", 
                PhoneNumber = "123", // Invalid length
                AcademicYear = 1
            } 
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid phone number: Invalid phone number length");

        _auditServiceMock.Verify(a => a.LogAsync(
            AuditAction.Error,
            "Student",
            null, null, null,
            "Invalid phone number during student creation: Invalid phone number length"), Times.Once);
            
        _studentRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Student>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_Should_ThrowException_WhenRepositoryFails()
    {
        // Arrange
        var command = new CreateStudentCommand 
        { 
            Student = new CreateStudentDto { Email = "test@example.com", AcademicYear = 1 } 
        };
        
        var exception = new DbUpdateException("Database error", new Exception());
        _studentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Student>())).ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>().WithMessage("Database error");
        
        _studentRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Student>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogCrudAsync(It.IsAny<AuditAction>(), null, It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }
}
