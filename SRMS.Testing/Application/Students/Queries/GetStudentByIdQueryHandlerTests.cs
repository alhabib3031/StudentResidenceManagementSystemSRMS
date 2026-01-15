using Moq;
using SRMS.Application.Students.GetStudentById;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Application.Students.DTOs;
using Xunit;
using FluentAssertions;
using SRMS.Domain.ValueObjects;

namespace SRMS.Testing.Application.Students.Queries;

public class GetStudentByIdQueryHandlerTests
{
    private readonly Mock<IRepositories<Student>> _studentRepositoryMock;
    private readonly GetStudentByIdQueryHandler _handler;

    public GetStudentByIdQueryHandlerTests()
    {
        _studentRepositoryMock = new Mock<IRepositories<Student>>();
        _handler = new GetStudentByIdQueryHandler(_studentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnStudentDto_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student 
        { 
            Id = studentId, 
            FirstName = "Test", 
            LastName = "User",
            Email = Email.Create("test@example.com")
        };
        var query = new GetStudentByIdQuery { Id = studentId };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<StudentDto>();
        result!.Id.Should().Be(studentId);
        result.FirstName.Should().Be("Test");
        result.Email.Should().Be("test@example.com");
        
        _studentRepositoryMock.Verify(r => r.GetByIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenStudentDoesNotExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentByIdQuery { Id = studentId };

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _studentRepositoryMock.Verify(r => r.GetByIdAsync(studentId), Times.Once);
    }
}
