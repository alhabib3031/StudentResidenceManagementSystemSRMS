using SRMS.Application.Payments.DTOs;

namespace SRMS.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetStudentPaymentsAsync(Guid studentId);
    Task<PaymentDetailsDto?> GetPaymentDetailsAsync(Guid paymentId);
    Task<bool> CreatePaymentAsync(CreatePaymentDto dto);
    Task<bool> UpdatePaymentStatusAsync(Guid paymentId, SRMS.Domain.Payments.Enums.PaymentStatus status);
    Task<decimal> GetTotalPaidAmountAsync(Guid studentId);
    Task<decimal> GetPendingDuesAmountAsync(Guid studentId);
}
