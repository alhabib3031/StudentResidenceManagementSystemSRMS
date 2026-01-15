using Moq;
using SRMS.Application.Managers.CreateManager;
using SRMS.Domain.Repositories;
using SRMS.Domain.Managers;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Managers.DTOs;
using Xunit;
using FluentAssertions;
using SRMS.Domain.Managers.Enums;

namespace SRMS.Testing.Application.Managers.Commands;

public class CreateManagerCommandHandlerTests
{
    private readonly Mock<IRepositories<Manager>> _managerRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly CreateManagerCommandHandler _handler;

    public CreateManagerCommandHandlerTests()
    {
        _managerRepositoryMock = new Mock<IRepositories<Manager>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new CreateManagerCommandHandler(_managerRepositoryMock.Object, _auditServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateManagerWithActiveStatus_WhenRequestIsValid()
    {
        // Arrange
        var createDto = new CreateManagerDto
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "manager@example.com",
            EmployeeNumber = "EMP123"
        };
        var command = new CreateManagerCommand { Manager = createDto };

        _managerRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Manager>()))
            .ReturnsAsync((Manager m) => m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ManagerStatus.Active);
        result.FullName.Should().Be("Admin User");
        result.EmployeeNumber.Should().Be("EMP123");

        _managerRepositoryMock.Verify(r => r.CreateAsync(It.Is<Manager>(m => 
            m.Status == ManagerStatus.Active &&
            m.EmployeeNumber == "EMP123"
        )), Times.Once);

        _auditServiceMock.Verify(a => a.LogCrudAsync(
            SRMS.Domain.AuditLogs.Enums.AuditAction.Create,
            null,
            It.Is<object>(o => o.GetType().GetProperty("FullName")!.GetValue(o)!.ToString() == "Admin User"),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenEmailIsInvalid()
    {
        // Arrange
        var createDto = new CreateManagerDto { Email = "invalid" };
        var command = new CreateManagerCommand { Manager = createDto };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid email: Invalid email format");
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var createDto = new CreateManagerDto { Email = "manager@example.com", PhoneNumber = "123" };
        var command = new CreateManagerCommand { Manager = createDto };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid phone number: Invalid phone number length");
    }
}
