using SRMS.Application.Payments.DTOs;
using SRMS.Application.Payments.Interfaces;
using SRMS.Domain.Payments;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.ValueObjects;

namespace SRMS.Infrastructure.Configurations.Services;

public class PaymentService : IPaymentService
{
    private readonly IRepositories<Payment> _paymentRepo;
    private readonly IRepositories<Reservation> _reservationRepo;

    public PaymentService(
        IRepositories<Payment> paymentRepo,
        IRepositories<Reservation> reservationRepo)
    {
        _paymentRepo = paymentRepo;
        _reservationRepo = reservationRepo;
    }

    public async Task<IEnumerable<PaymentDto>> GetStudentPaymentsAsync(Guid studentId)
    {
        var reservations = await _reservationRepo.Query()
            .Where(r => r.StudentId == studentId)
            .Include(r => r.Student)
            .ToListAsync();

        var reservationIds = reservations.Select(r => r.Id).ToList();

        var payments = await _paymentRepo.FindAsync(p => p.ReservationId.HasValue && reservationIds.Contains(p.ReservationId.Value));

        return payments.Select(p =>
        {
            var res = reservations.FirstOrDefault(r => r.Id == p.ReservationId);
            return new PaymentDto
            {
                Id = p.Id,
                ReservationId = p.ReservationId!.Value,
                StudentName = res?.Student?.FullName ?? "N/A",
                Amount = p.Amount?.Amount ?? 0,
                Currency = p.Amount?.Currency ?? "LYD",
                Description = p.Description,
                Status = p.Status,
                Month = p.Month,
                Year = p.Year,
                DueDate = p.DueDate,
                PaidAt = p.PaidAt,
                TransactionId = p.TransactionId,
                PaymentMethod = p.PaymentMethod,
                Reference = p.PaymentReference,
                CreatedAt = p.CreatedAt
            };
        }).OrderByDescending(p => p.DueDate).ToList();
    }

    public async Task<PaymentDetailsDto?> GetPaymentDetailsAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.Query()
            .Where(p => p.Id == paymentId)
            .Include(p => p.Reservation)
            .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync();

        if (payment == null) return null;

        return new PaymentDetailsDto
        {
            Id = payment.Id,
            StudentId = payment.Reservation?.StudentId ?? Guid.Empty,
            StudentName = payment.Reservation?.Student?.FullName ?? "N/A",
            StudentEmail = payment.Reservation?.Student?.Email?.Value,
            StudentPhone = payment.Reservation?.Student?.PhoneNumber?.Value,
            AmountValue = payment.Amount?.Amount ?? 0,
            AmountCurrency = payment.Amount?.Currency ?? "LYD",
            AmountFormatted = $"{payment.Amount?.Amount ?? 0} {payment.Amount?.Currency ?? "LYD"}",
            Description = payment.Description,
            Status = payment.Status,
            Month = payment.Month,
            Year = payment.Year,
            DueDate = payment.DueDate,
            PaidAt = payment.PaidAt,
            TransactionId = payment.TransactionId,
            PaymentMethod = payment.PaymentMethod,
            PaymentReference = payment.PaymentReference,
            Notes = payment.Notes,
            LateFeeValue = payment.LateFee?.Amount ?? 0,
            LateFeeCurrency = payment.LateFee?.Currency ?? "LYD",
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }

    public async Task<bool> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ReservationId = dto.ReservationId,
            Amount = Money.Create(dto.Amount, dto.Currency),
            Description = dto.Description,
            Status = PaymentStatus.Pending,
            DueDate = dto.DueDate,
            PaymentReference = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            Month = dto.Month,
            Year = dto.Year,
            Notes = dto.Notes,
            LateFee = dto.LateFeeAmount.HasValue ? Money.Create(dto.LateFeeAmount.Value, dto.LateFeeCurrency ?? "LYD") : null
        };

        var result = await _paymentRepo.CreateAsync(payment);
        return result != null;
    }

    public async Task<bool> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.Status = status;
        if (status == PaymentStatus.Paid)
        {
            payment.PaidAt = DateTime.UtcNow;
        }

        var result = await _paymentRepo.UpdateAsync(payment);
        return result != null;
    }

    public async Task<decimal> GetTotalPaidAmountAsync(Guid studentId)
    {
        var reservationIds = await _reservationRepo.Query()
            .Where(r => r.StudentId == studentId)
            .Select(r => r.Id)
            .ToListAsync();

        var payments = await _paymentRepo.FindAsync(p => p.ReservationId.HasValue && reservationIds.Contains(p.ReservationId.Value) && p.Status == PaymentStatus.Paid);
        return payments.Sum(p => p.Amount?.Amount ?? 0);
    }

    public async Task<decimal> GetPendingDuesAmountAsync(Guid studentId)
    {
        var reservationIds = await _reservationRepo.Query()
            .Where(r => r.StudentId == studentId)
            .Select(r => r.Id)
            .ToListAsync();

        var payments = await _paymentRepo.FindAsync(p => p.ReservationId.HasValue && reservationIds.Contains(p.ReservationId.Value) && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue));
        return payments.Sum(p => p.Amount?.Amount ?? 0);
    }

    // Implementation for IPaymentService.ProcessDummyPaymentAsync
    public Task<Guid?> ProcessDummyPaymentAsync(PaymentRequestDto request)
    {
        // For a dummy payment, we can simulate success by returning a new Guid
        // In a real scenario, this would involve integrating with a payment gateway
        return Task.FromResult<Guid?>(Guid.NewGuid());
    }
}
