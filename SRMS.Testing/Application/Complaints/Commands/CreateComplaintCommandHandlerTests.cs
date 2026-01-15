using Moq;
using SRMS.Application.Complaints.CreateComplaint;
using SRMS.Domain.Repositories;
using SRMS.Domain.Complaints;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Application.Complaints.DTOs;
using Xunit;
using FluentAssertions;
using SRMS.Domain.Complaints.Enums;
using SRMS.Application.Notifications.DTOs;
using SRMS.Domain.Notifications.Enums;

namespace SRMS.Testing.Application.Complaints.Commands;

public class CreateComplaintCommandHandlerTests
{
    private readonly Mock<IRepositories<Complaint>> _complaintRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CreateComplaintCommandHandler _handler;

    public CreateComplaintCommandHandlerTests()
    {
        _complaintRepositoryMock = new Mock<IRepositories<Complaint>>();
        _auditServiceMock = new Mock<IAuditService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _handler = new CreateComplaintCommandHandler(
            _complaintRepositoryMock.Object,
            _auditServiceMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateComplaint_LogAudit_And_SendNotification()
    {
        // Arrange
        var createDto = new CreateComplaintDto
        {
            ReservationId = Guid.NewGuid(),
            Title = "Leaky Faucet",
            Description = "The faucet in the bathroom is leaking.",
            ComplaintTypeId = Guid.NewGuid(),
            Priority = ComplaintPriority.Medium
        };
        var command = new CreateComplaintCommand(createDto);

        _complaintRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Complaint>()))
            .ReturnsAsync((Complaint c) => c);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Leaky Faucet");
        result.Status.Should().Be(ComplaintStatus.Open);
        result.ComplaintNumber.Should().StartWith("CMP-");

        // Verify Repository was called
        _complaintRepositoryMock.Verify(r => r.CreateAsync(It.Is<Complaint>(c => c.Title == "Leaky Faucet")), Times.Once);

        // Verify Audit was logged
        // FIX: Swapped `newValues` and `oldValues` to match the actual call signature.
        _auditServiceMock.Verify(a => a.LogAsync(
            SRMS.Domain.AuditLogs.Enums.AuditAction.ComplaintSubmitted,
            "Complaint",
            It.IsAny<string>(),
            null, // oldValues
            It.IsAny<object>(), // newValues
            It.Is<string>(s => s.Contains("Complaint submitted"))
        ), Times.Once);

        // Verify Notification was sent
        _notificationServiceMock.Verify(n => n.SendNotificationToRoleAsync(
            "Manager",
            It.Is<CreateNotificationDto>(dto => 
                dto.Title == "New Complaint Submitted" &&
                dto.Message.Contains("Leaky Faucet") &&
                dto.RelatedEntityType == "Complaint"
            )
        ), Times.Once);
    }

    [Theory]
    [InlineData(ComplaintPriority.Critical, NotificationPriority.High)]
    [InlineData(ComplaintPriority.High, NotificationPriority.Low)]
    [InlineData(ComplaintPriority.Medium, NotificationPriority.Low)]
    [InlineData(ComplaintPriority.Low, NotificationPriority.Low)]
    public async Task Handle_Should_MapComplaintPriorityToNotificationPriorityCorrectly(ComplaintPriority complaintPriority, NotificationPriority expectedNotificationPriority)
    {
        // Arrange
        var createDto = new CreateComplaintDto { Priority = complaintPriority, Title = "Test" };
        var command = new CreateComplaintCommand(createDto);

        _complaintRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Complaint>()))
            .ReturnsAsync((Complaint c) => c);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(n => n.SendNotificationToRoleAsync(
            It.IsAny<string>(),
            It.Is<CreateNotificationDto>(dto => dto.Priority == expectedNotificationPriority)
        ), Times.Once);
    }
}
