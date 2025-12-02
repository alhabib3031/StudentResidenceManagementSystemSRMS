using MediatR;
using SRMS.Application.Payments.DTOs;

namespace SRMS.Application.Payments.UpdatePayment;

public class UpdatePaymentCommand : IRequest<PaymentDto?>
{
    public UpdatePaymentDto Payment { get; set; } = new();
}