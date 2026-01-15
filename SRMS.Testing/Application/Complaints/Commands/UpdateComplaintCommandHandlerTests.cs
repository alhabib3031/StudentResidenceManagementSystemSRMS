using Moq;
using SRMS.Application.Complaints.UpdateComplaint;
using SRMS.Domain.Repositories;
using SRMS.Domain.Complaints;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Complaints.DTOs;
using Xunit;
using FluentAssertions;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Testing.Application.Complaints.Commands;

public class UpdateComplaintCommandHandlerTests
{
    private readonly Mock<IRepositories<Complaint>> _complaintRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly UpdateComplaintCommandHandler _handler;

    public UpdateComplaintCommandHandlerTests()
    {
        _complaintRepositoryMock = new Mock<IRepositories<Complaint>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new UpdateComplaintCommandHandler(_complaintRepositoryMock.Object, _auditServiceMock.Object);
    }

    private Complaint CreateSampleComplaint()
    {
        return new Complaint
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            Status = ComplaintStatus.Open,
            ComplaintType = new ComplaintType { Name = "General" }
        };
    }

    [Fact]
    public async Task Handle_Should_UpdateComplaint_WhenRequestIsValid()
    {
        // Arrange
        var existingComplaint = CreateSampleComplaint();
        var updateDto = new UpdateComplaintDto { Id = existingComplaint.Id, Title = "New Title", Status = ComplaintStatus.InProgress };
        var command = new UpdateComplaintCommand { Complaint = updateDto };

        _complaintRepositoryMock.Setup(r => r.GetByIdAsync(existingComplaint.Id)).ReturnsAsync(existingComplaint);
        _complaintRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Complaint>())).ReturnsAsync((Complaint c) => c);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        _complaintRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Complaint>(c => c.Title == "New Title")), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_SetResolvedAt_WhenStatusChangesToResolved()
    {
        // Arrange
        var existingComplaint = CreateSampleComplaint(); // Status is Open, ResolvedAt is null
        var updateDto = new UpdateComplaintDto { Id = existingComplaint.Id, Status = ComplaintStatus.Resolved };
        var command = new UpdateComplaintCommand { Complaint = updateDto };

        _complaintRepositoryMock.Setup(r => r.GetByIdAsync(existingComplaint.Id)).ReturnsAsync(existingComplaint);
        _complaintRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Complaint>())).ReturnsAsync((Complaint c) => c);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _complaintRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Complaint>(c => 
            c.Status == ComplaintStatus.Resolved &&
            c.ResolvedAt.HasValue
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_SetAssignedAt_WhenComplaintIsAssigned()
    {
        // Arrange
        var existingComplaint = CreateSampleComplaint(); // AssignedTo is null, AssignedAt is null
        var managerId = Guid.NewGuid();
        var updateDto = new UpdateComplaintDto { Id = existingComplaint.Id, AssignedTo = managerId };
        var command = new UpdateComplaintCommand { Complaint = updateDto };

        _complaintRepositoryMock.Setup(r => r.GetByIdAsync(existingComplaint.Id)).ReturnsAsync(existingComplaint);
        _complaintRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Complaint>())).ReturnsAsync((Complaint c) => c);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _complaintRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Complaint>(c => 
            c.AssignedTo == managerId &&
            c.AssignedAt.HasValue
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_LogSpecificAudit_WhenStatusChanges()
    {
        // Arrange
        var existingComplaint = CreateSampleComplaint(); // Status is Open
        var updateDto = new UpdateComplaintDto { Id = existingComplaint.Id, Status = ComplaintStatus.Closed };
        var command = new UpdateComplaintCommand { Complaint = updateDto };

        _complaintRepositoryMock.Setup(r => r.GetByIdAsync(existingComplaint.Id)).ReturnsAsync(existingComplaint);
        _complaintRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Complaint>())).ReturnsAsync((Complaint c) => c);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify the general update log
        _auditServiceMock.Verify(a => a.LogCrudAsync(AuditAction.Update, It.IsAny<object>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        
        // Verify the specific status change log
        _auditServiceMock.Verify(a => a.LogAsync(
            AuditAction.ComplaintClosed,
            "Complaint",
            existingComplaint.Id.ToString(),
            It.IsAny<object>(),
            It.IsAny<object>(),
            It.Is<string>(s => s.Contains("status changed from Open to Closed"))
        ), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_ReturnNull_WhenComplaintDoesNotExist()
    {
        // Arrange
        var command = new UpdateComplaintCommand { Complaint = new UpdateComplaintDto { Id = Guid.NewGuid() } };
        _complaintRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _auditServiceMock.Verify(a => a.LogAsync(AuditAction.Failure, "Complaint", command.Complaint.Id.ToString(), null, null, "Attempted to update non-existent complaint"), Times.Once);
    }
}
