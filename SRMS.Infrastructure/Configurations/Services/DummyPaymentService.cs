using MapsterMapper;
using SRMS.Application.Payments.DTOs;
using SRMS.Application.Payments.Interfaces;
using SRMS.Domain.Payments;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.ValueObjects;

namespace SRMS.Infrastructure.Configurations.Services;

public class DummyPaymentService : IPaymentService
{
    private readonly IRepositories<Payment> _paymentRepository;
    private readonly IMapper _mapper;

    public DummyPaymentService(IRepositories<Payment> paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Guid?> ProcessDummyPaymentAsync(PaymentRequestDto request)
    {
        // In a real application, this would involve integrating with a payment gateway.
        // For this dummy implementation, we simulate a successful payment.

        var payment = new Payment
        {
            // Note: ReservationId is null initially, will be updated by ReservationService
            Amount = Money.Create(request.Amount, request.Currency),
            Description = request.Description,
            Status = PaymentStatus.Paid, // Simulate successful payment
            PaidAt = DateTime.UtcNow,
            TransactionId = Guid.NewGuid().ToString(), // Generate a dummy transaction ID
            PaymentMethod = "Dummy Gateway",
            PaymentReference = $"DUMMY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year,
            DueDate = DateTime.UtcNow.AddMonths(1) // Dummy due date for example
        };

        await _paymentRepository.CreateAsync(payment);
        await _paymentRepository.SaveChangesAsync();

        return payment.Id; // Return the ID of the created payment
    }

    public Task<IEnumerable<PaymentDto>> GetStudentPaymentsAsync(Guid studentId)
    {
        return Task.FromResult<IEnumerable<PaymentDto>>(new List<PaymentDto>());
    }

    public Task<decimal> GetPendingDuesAmountAsync(Guid studentId)
    {
        return Task.FromResult(0m);
    }

    public Task<decimal> GetTotalPaidAmountAsync(Guid studentId)
    {
        return Task.FromResult(0m);
    }
}