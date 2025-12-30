using MapsterMapper;
using SRMS.Application.Payments.Interfaces; // For IPaymentService
using SRMS.Application.Reservations.DTOs;
using SRMS.Application.Reservations.Interfaces;
using SRMS.Application.Rooms.Interfaces; // For IRoomPricingService
using SRMS.Domain.Repositories;
using SRMS.Domain.Reservations;
using SRMS.Domain.Reservations.Enums;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.Payments;
using SRMS.Domain.Residences;
using SRMS.Domain.Payments.Enums;
using SRMS.Application.Payments.DTOs; // For Residence

namespace SRMS.Infrastructure.Configurations.Services;

public class ReservationService : IReservationService
{
    private readonly IRepositories<Reservation> _reservationRepository;
    private readonly IRepositories<Room> _roomRepository;
    private readonly IRepositories<Student> _studentRepository;
    private readonly IRepositories<Payment> _paymentRepository;
    private readonly IRepositories<Residence> _residenceRepository; // New
    private readonly IRoomPricingService _roomPricingService; // New
    private readonly IPaymentService _paymentService; // New
    private readonly IMapper _mapper;

    public ReservationService(
        IRepositories<Reservation> reservationRepository,
        IRepositories<Room> roomRepository,
        IRepositories<Student> studentRepository,
        IRepositories<Payment> paymentRepository,
        IRepositories<Residence> residenceRepository, // New
        IRoomPricingService roomPricingService, // New
        IPaymentService paymentService, // New
        IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _studentRepository = studentRepository;
        _paymentRepository = paymentRepository;
        _residenceRepository = residenceRepository; // New
        _roomPricingService = roomPricingService; // New
        _paymentService = paymentService; // New
        _mapper = mapper;
    }

    public async Task<Reservation> CreateReservationAsync(ReservationRequestDto request)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new Exception("Student not found.");
        }

        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        if (room == null)
        {
            throw new Exception("Room not found.");
        }

        if (room.IsFull)
        {
            throw new Exception("Room is already full.");
        }

        if (room.OccupiedBeds >= room.TotalBeds)
        {
            throw new Exception("Room has no available beds.");
        }

        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
        if (payment == null || payment.Status != PaymentStatus.Paid)
        {
            throw new Exception("Payment not found or not completed.");
        }

        // Check for existing reservations for the student and room within the date range (simple check for now)
        var existingReservation = (await _reservationRepository.FindAsync(r =>
            r.StudentId == request.StudentId && r.RoomId == request.RoomId &&
            r.Status != ReservationStatus.Cancelled &&
            ((request.StartDate >= r.StartDate && request.StartDate < r.EndDate) ||
             (request.EndDate > r.StartDate && request.EndDate <= r.EndDate) ||
             (request.StartDate <= r.StartDate && request.EndDate >= r.EndDate))
        )).FirstOrDefault();

        if (existingReservation != null)
        {
            throw new Exception("Student already has an active reservation for this room or overlapping dates.");
        }

        var reservation = new Reservation
        {
            StudentId = request.StudentId,
            RoomId = request.RoomId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ReservationDate = DateTime.UtcNow,
            IsPaid = true, // Marked as paid because payment was processed
            TotalAmount = payment.Amount, // Use the amount from the successful payment
            Status = ReservationStatus.Confirmed
        };

        // Update room's occupied beds
        room.OccupiedBeds++;
        await _roomRepository.UpdateAsync(room);

        // Link payment to this reservation
        payment.ReservationId = reservation.Id; // Will be set after reservation is created and has an ID
        await _paymentRepository.UpdateAsync(payment);

        var createdReservation = await _reservationRepository.CreateAsync(reservation);
        await _reservationRepository.SaveChangesAsync();
        await _roomRepository.SaveChangesAsync(); // Save room changes
        await _paymentRepository.SaveChangesAsync(); // Save payment changes

        return createdReservation;
    }

    public async Task<Reservation?> GetReservationByIdAsync(Guid reservationId)
    {
        return await _reservationRepository.GetByIdAsync(reservationId);
    }

    public async Task<List<Reservation>> GetStudentReservationsAsync(Guid studentId)
    {
        var reservations = await _reservationRepository.FindAsync(r => r.StudentId == studentId);
        return reservations.ToList();
    }

    public async Task<Reservation> UpdateReservationStatusAsync(Guid reservationId, ReservationStatus newStatus)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new Exception("Reservation not found.");
        }

        reservation.Status = newStatus;
        var updatedReservation = await _reservationRepository.UpdateAsync(reservation);
        await _reservationRepository.SaveChangesAsync();
        return updatedReservation;
    }

    public async Task<IEnumerable<SRMS.Application.Residences.DTOs.ResidenceDto>> GetAvailableResidencesAsync()
    {
        var residences = await _residenceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SRMS.Application.Residences.DTOs.ResidenceDto>>(residences);
    }

    public async Task<IEnumerable<RoomAvailabilityDto>> GetVacantRoomsByResidenceAsync(Guid residenceId)
    {
        // Fetch rooms for the residence that are not fully occupied and are in Available status
        var vacantRooms = await _roomRepository.FindAsync(r =>
            r.ResidenceId == residenceId &&
            r.Status == Domain.Rooms.Enums.RoomStatus.Available &&
            r.OccupiedBeds < r.TotalBeds);

        // Manual mapping to avoid Mapster issues
        return vacantRooms.Select(r => new RoomAvailabilityDto(
            r.Id,
            r.RoomNumber,
            r.Floor,
            r.RoomType,
            r.TotalBeds,
            r.OccupiedBeds,
            r.MonthlyRent, // Pass directly
            r.Status
        ));
    }

    public async Task<ReserveRoomResponse> ReserveRoomAsync(ReserveRoomRequest request)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new Exception("Student not found.");
        }

        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        if (room == null)
        {
            throw new Exception("Room not found.");
        }

        if (room.ResidenceId != request.ResidenceId)
        {
            throw new Exception("Room does not belong to the specified residence.");
        }

        if (room.Status != Domain.Rooms.Enums.RoomStatus.Available || room.IsFull)
        {
            throw new Exception("Room is not available for reservation.");
        }

        // Calculate fee
        var fee = await _roomPricingService.CalculateRoomFee(room, student);

        // Process dummy payment
        var paymentRequest = new PaymentRequestDto
        {
            StudentId = student.Id,
            Amount = fee.Amount!.Value,
            Currency = fee.Currency,
            Description = $"Room reservation for {room.RoomNumber} in {room.ResidenceId}"
        };
        var paymentId = await _paymentService.ProcessDummyPaymentAsync(paymentRequest);

        if (paymentId == null)
        {
            throw new Exception("Payment failed or could not be processed.");
        }

        // Update room status and occupied beds
        room.OccupiedBeds++;
        room.Status = room.OccupiedBeds == room.TotalBeds ? Domain.Rooms.Enums.RoomStatus.Occupied : Domain.Rooms.Enums.RoomStatus.Available; // If all beds are taken, mark as occupied
        await _roomRepository.UpdateAsync(room);

        // Update residence available capacity
        var residence = await _residenceRepository.GetByIdAsync(request.ResidenceId);
        if (residence != null)
        {
            residence.AvailableCapacity--;
            await _residenceRepository.UpdateAsync(residence);
        }

        // Create reservation
        var reservation = new Reservation
        {
            StudentId = request.StudentId,
            RoomId = request.RoomId,
            ResidenceId = request.ResidenceId, // Add ResidenceId to Reservation
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = DateOnly.FromDateTime(request.EndDate),
            ReservationDate = DateTime.UtcNow,
            IsPaid = true,
            TotalAmount = fee,
            Status = ReservationStatus.Confirmed
        };

        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Ensure the payment record is updated with the reservation ID if needed.
        // This was initially handled in DummyPaymentService, but for full linkage,
        // we might retrieve and update it here or rely on the payment service to handle it.
        // For now, assuming payment service correctly links or it's not critical for dummy.

        // Update student status to Suspended (or Pending) for Registrar verification
        student.Status = Domain.Students.Enums.StudentStatus.Suspended; // "معلق" until verified
        await _studentRepository.UpdateAsync(student);

        await _roomRepository.SaveChangesAsync();
        await _residenceRepository.SaveChangesAsync();
        await _reservationRepository.SaveChangesAsync();
        await _studentRepository.SaveChangesAsync();

        return new ReserveRoomResponse(
            createdReservation.Id,
            paymentId.Value,
            fee,
            createdReservation.Status.ToString()
        );
    }
}
