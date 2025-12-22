using SRMS.Application.Payments.DTOs;
using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<Guid?> ProcessDummyPaymentAsync(PaymentRequestDto request);
    Task<IEnumerable<PaymentDto>> GetStudentPaymentsAsync(Guid studentId);
    Task<decimal> GetPendingDuesAmountAsync(Guid studentId);
    Task<decimal> GetTotalPaidAmountAsync(Guid studentId);
}