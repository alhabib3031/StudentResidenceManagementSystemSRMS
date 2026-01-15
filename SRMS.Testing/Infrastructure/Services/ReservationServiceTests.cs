using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Repositories;
using SRMS.Infrastructure.Configurations.Services;
using Xunit;
using FluentAssertions;
using Moq;
using MapsterMapper;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.Reservations;
using SRMS.Domain.Payments;
using SRMS.Application.Reservations.DTOs;
using SRMS.Application.Rooms.Interfaces;
using SRMS.Application.Payments.Interfaces;
using SRMS.Application.Payments.DTOs;
using SRMS.Domain.ValueObjects;
using SRMS.Domain.Students.Enums;
using Microsoft.AspNetCore.Http;
using SRMS.Domain.Rooms.Enums;

namespace SRMS.Testing.Infrastructure.Services;

public class ReservationServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ReservationService _reservationService;
    private readonly GenericRepository<Residence> _residenceRepository;
    private readonly GenericRepository<Room> _roomRepository;
    private readonly GenericRepository<Student> _studentRepository;
    private readonly Mock<IRoomPricingService> _pricingServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;

    public ReservationServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock
            .Setup(h => h.HttpContext)
            .Returns(new DefaultHttpContext());

        var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
        contextFactoryMock.Setup(f => f.CreateDbContext())
            .Returns(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));

        contextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));

        _residenceRepository = new GenericRepository<Residence>(contextFactoryMock.Object);
        _roomRepository = new GenericRepository<Room>(contextFactoryMock.Object);
        _studentRepository = new GenericRepository<Student>(contextFactoryMock.Object);
        var paymentRepository = new GenericRepository<Payment>(contextFactoryMock.Object);

        _pricingServiceMock = new Mock<IRoomPricingService>();
        _paymentServiceMock = new Mock<IPaymentService>();

        var mapper = new Mock<IMapper>();

        _reservationService = new ReservationService(
            new GenericRepository<Reservation>(contextFactoryMock.Object),
            _roomRepository,
            _studentRepository,
            paymentRepository,
            _residenceRepository,
            _pricingServiceMock.Object,
            _paymentServiceMock.Object,
            mapper.Object
        );
    }

    private ApplicationDbContext CreateContext()
        => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    // ✅ الإصلاح هنا
    private async Task<(Student, Residence, Room)> SeedEntitiesAsync()
    {
        var student = await _studentRepository.CreateAsync(
            new Student
            {
                FirstName = "Test",
                Status = StudentStatus.Active
            });

        var residence = await _residenceRepository.CreateAsync(
            new Residence
            {
                Name = "Test Residence",
                AvailableCapacity = 10
            });

        var room = new Room
        {
            ResidenceId = residence.Id,
            Residence = residence, // 🔴 مهم جداً
            RoomNumber = "101",
            TotalBeds = 2,
            OccupiedBeds = 0,
            Status = RoomStatus.Available,
            Floor = 1,
            RoomType = RoomType.Double,
            MonthlyRent = Money.Create(500, "USD")
        };

        room = await _roomRepository.CreateAsync(room);

        return (student, residence, room);
    }

    [Fact]
    public async Task ReserveRoomAsync_Should_CreateReservation_AndUpdateEntities_OnSuccess()
    {
        // Arrange
        var (student, residence, room) = await SeedEntitiesAsync();

        var request = new ReserveRoomRequest(
            student.Id,
            residence.Id,
            room.Id,
            DateTime.Now,
            DateTime.Now.AddMonths(6));

        _pricingServiceMock
            .Setup(p => p.CalculateRoomFee(It.IsAny<Room>(), It.IsAny<Student>()))
            .ReturnsAsync(Money.Create(500, "USD"));

        _paymentServiceMock
            .Setup(p => p.ProcessDummyPaymentAsync(It.IsAny<PaymentRequestDto>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var response = await _reservationService.ReserveRoomAsync(request);

        await using var context = CreateContext();
        var reservationInDb = await context.Reservations.FindAsync(response.ReservationId);
        var roomInDb = await context.Rooms.FindAsync(room.Id);
        var residenceInDb = await context.Residences.FindAsync(residence.Id);
        var studentInDb = await context.Students.FindAsync(student.Id);

        // Assert
        response.Should().NotBeNull();
        reservationInDb.Should().NotBeNull();
        reservationInDb!.Status.Should().Be(
            Domain.Reservations.Enums.ReservationStatus.Confirmed);

        roomInDb!.OccupiedBeds.Should().Be(1);
        residenceInDb!.AvailableCapacity.Should().Be(9);
        studentInDb!.Status.Should().Be(StudentStatus.Suspended);
    }

    [Fact]
    public async Task ReserveRoomAsync_Should_ThrowException_WhenRoomIsFull()
    {
        // Arrange
        var (student, residence, room) = await SeedEntitiesAsync();

        room.OccupiedBeds = room.TotalBeds;
        await _roomRepository.UpdateAsync(room);

        var request = new ReserveRoomRequest(
            student.Id,
            residence.Id,
            room.Id,
            DateTime.Now,
            DateTime.Now.AddMonths(6));

        // Act
        Func<Task> act = async () =>
            await _reservationService.ReserveRoomAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Room is not available for reservation.");
    }

    [Fact]
    public async Task ReserveRoomAsync_Should_ThrowException_WhenPaymentFails()
    {
        // Arrange
        var (student, residence, room) = await SeedEntitiesAsync();

        var request = new ReserveRoomRequest(
            student.Id,
            residence.Id,
            room.Id,
            DateTime.Now,
            DateTime.Now.AddMonths(6));

        _pricingServiceMock
            .Setup(p => p.CalculateRoomFee(It.IsAny<Room>(), It.IsAny<Student>()))
            .ReturnsAsync(Money.Create(500, "USD"));

        _paymentServiceMock
            .Setup(p => p.ProcessDummyPaymentAsync(It.IsAny<PaymentRequestDto>()))
            .ReturnsAsync((Guid?)null);

        // Act
        Func<Task> act = async () =>
            await _reservationService.ReserveRoomAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Payment failed or could not be processed.");
    }
}
