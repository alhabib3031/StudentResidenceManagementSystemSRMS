using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Repositories;
using SRMS.Infrastructure.Configurations.Services;
using Xunit;
using FluentAssertions;
using Moq;
using SRMS.Domain.Payments;
using SRMS.Domain.Reservations;
using SRMS.Domain.Students;
using SRMS.Application.Payments.DTOs;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace SRMS.Testing.Infrastructure.Services;

public class PaymentServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly PaymentService _paymentService;
    private readonly GenericRepository<Payment> _paymentRepository;
    private readonly GenericRepository<Reservation> _reservationRepository;
    private readonly GenericRepository<Student> _studentRepository;

    public PaymentServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

        var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
        contextFactoryMock.Setup(f => f.CreateDbContext())
            .Returns(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));
        contextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));

        _paymentRepository = new GenericRepository<Payment>(contextFactoryMock.Object);
        _reservationRepository = new GenericRepository<Reservation>(contextFactoryMock.Object);
        _studentRepository = new GenericRepository<Student>(contextFactoryMock.Object);

        _paymentService = new PaymentService(_paymentRepository, _reservationRepository);
    }
    
    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    private async Task<(Student, Reservation)> SeedStudentAndReservationAsync()
    {
        var student = await _studentRepository.CreateAsync(new Student { FirstName = "Test" });
        var reservation = await _reservationRepository.CreateAsync(new Reservation { StudentId = student.Id });
        return (student, reservation);
    }

    [Fact]
    public async Task CreatePaymentAsync_Should_AddPaymentToDatabase()
    {
        // Arrange
        var (_, reservation) = await SeedStudentAndReservationAsync();
        var createDto = new CreatePaymentDto { ReservationId = reservation.Id, Amount = 150, Currency = "USD" };

        // Act
        var result = await _paymentService.CreatePaymentAsync(createDto);
        
        await using var context = CreateContext();
        var paymentInDb = await context.Payments.FirstOrDefaultAsync(p => p.ReservationId == reservation.Id);

        // Assert
        result.Should().BeTrue();
        paymentInDb.Should().NotBeNull();
        paymentInDb!.Amount.Amount.Should().Be(150);
        paymentInDb.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_Should_ChangeStatusAndSetPaidAt_WhenStatusIsPaid()
    {
        // Arrange
        var (_, reservation) = await SeedStudentAndReservationAsync();
        var payment = await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Status = PaymentStatus.Pending });

        // Act
        var result = await _paymentService.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.Paid);
        
        await using var context = CreateContext();
        var paymentInDb = await context.Payments.FindAsync(payment.Id);

        // Assert
        result.Should().BeTrue();
        paymentInDb.Should().NotBeNull();
        paymentInDb!.Status.Should().Be(PaymentStatus.Paid);
        paymentInDb.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetTotalPaidAmountAsync_Should_ReturnCorrectSumOfPaidPayments()
    {
        // Arrange
        var (student, reservation) = await SeedStudentAndReservationAsync();
        await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Amount = Money.Create(100), Status = PaymentStatus.Paid });
        await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Amount = Money.Create(50), Status = PaymentStatus.Paid });
        await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Amount = Money.Create(200), Status = PaymentStatus.Pending });

        // Act
        var totalPaid = await _paymentService.GetTotalPaidAmountAsync(student.Id);

        // Assert
        totalPaid.Should().Be(150);
    }

    [Fact]
    public async Task GetPendingDuesAmountAsync_Should_ReturnCorrectSumOfPendingAndOverduePayments()
    {
        // Arrange
        var (student, reservation) = await SeedStudentAndReservationAsync();
        await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Amount = Money.Create(100), Status = PaymentStatus.Paid });
        await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Amount = Money.Create(200), Status = PaymentStatus.Pending });
        await _paymentRepository.CreateAsync(new Payment { ReservationId = reservation.Id, Amount = Money.Create(75), Status = PaymentStatus.Overdue });

        // Act
        var totalDue = await _paymentService.GetPendingDuesAmountAsync(student.Id);

        // Assert
        totalDue.Should().Be(275);
    }
}
