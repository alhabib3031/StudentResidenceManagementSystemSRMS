using Moq;
using SRMS.Application.Payments.CreatePayment;
using SRMS.Domain.Repositories;
using SRMS.Domain.Payments;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Payments.DTOs;
using Xunit;
using FluentAssertions;
using SRMS.Domain.Payments.Enums;

namespace SRMS.Testing.Application.Payments.Commands;

public class CreatePaymentCommandHandlerTests
{
    private readonly Mock<IRepositories<Payment>> _paymentRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IRepositories<Payment>>();
        _auditServiceMock = new Mock<IAuditService>();
        _handler = new CreatePaymentCommandHandler(_paymentRepositoryMock.Object, _auditServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreatePaymentWithPendingStatus_WhenRequestIsValid()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var createDto = new CreatePaymentDto
        {
            ReservationId = reservationId,
            Amount = 100,
            Currency = "USD",
            Description = "Monthly Rent",
            Month = 4,
            Year = 2024,
            DueDate = DateTime.UtcNow.AddDays(10)
        };
        var command = new CreatePaymentCommand { Payment = createDto };

        _paymentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Pending);
        result.Amount.Should().Be(100);
        result.ReservationId.Should().Be(reservationId);

        _paymentRepositoryMock.Verify(r => r.CreateAsync(It.Is<Payment>(p => 
            p.Status == PaymentStatus.Pending &&
            p.PaymentReference.StartsWith("PAY-")
        )), Times.Once);

        _auditServiceMock.Verify(a => a.LogPaymentAsync(
            It.IsAny<Guid>(),
            "Created",
            100,
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenAmountIsNegative()
    {
        // Arrange
        var createDto = new CreatePaymentDto { Amount = -50, Month = 1, Year = 2024 };
        var command = new CreatePaymentCommand { Payment = createDto };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid amount: Amount cannot be negative");
    }

    [Fact]
    public async Task Handle_Should_CreatePaymentWithLateFee_WhenLateFeeIsProvided()
    {
        // Arrange
        var createDto = new CreatePaymentDto
        {
            Amount = 100,
            LateFeeAmount = 10,
            LateFeeCurrency = "USD",
            Month = 1, // FIX: Added valid month
            Year = 2024
        };
        var command = new CreatePaymentCommand { Payment = createDto };

        _paymentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentRepositoryMock.Verify(r => r.CreateAsync(It.Is<Payment>(p =>
            p.LateFee != null &&
            p.LateFee.Amount == 10
        )), Times.Once);
    }
}
