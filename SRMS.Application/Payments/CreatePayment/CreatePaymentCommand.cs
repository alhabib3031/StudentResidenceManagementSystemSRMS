using MediatR;
using SRMS.Application.Payments.DTOs;

namespace SRMS.Application.Payments.CreatePayment;

public class CreatePaymentCommand : IRequest<PaymentDto>
{
    public CreatePaymentDto Payment { get; set; } = new();
}