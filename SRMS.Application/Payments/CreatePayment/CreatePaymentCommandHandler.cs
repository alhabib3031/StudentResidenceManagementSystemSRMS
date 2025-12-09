using System.Globalization;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Payments.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Payments;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Payments.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IRepositories<Payment> _paymentRepository;
    private readonly IAuditService _audit;

    public CreatePaymentCommandHandler(IRepositories<Payment> paymentRepository, IAuditService audit)
    {
        _paymentRepository = paymentRepository;
        _audit = audit;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = request.Payment.StudentId,
            Description = request.Payment.Description,
            Status = PaymentStatus.Pending,
            Month = request.Payment.Month,
            Year = request.Payment.Year,
            DueDate = request.Payment.DueDate,
            PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Notes = request.Payment.Notes,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };

        // Amount (Value Object)
        try
        {
            payment.Amount = Money.Create(request.Payment.Amount, request.Payment.Currency);
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Payment",
                additionalInfo: $"Invalid amount during payment creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid amount: {ex.Message}");
        }

        // Late Fee (Value Object)
        if (request.Payment.LateFeeAmount.HasValue && request.Payment.LateFeeAmount.Value > 0)
        {
            try
            {
                payment.LateFee = Money.Create(
                    request.Payment.LateFeeAmount.Value,
                    request.Payment.LateFeeCurrency ?? "LYD"
                );
            }
            catch (ArgumentException ex)
            {
                await _audit.LogAsync(
                    AuditAction.Error,
                    "Payment",
                    additionalInfo: $"Invalid late fee during payment creation: {ex.Message}"
                );
                throw new InvalidOperationException($"Invalid late fee: {ex.Message}");
            }
        }

        var created = await _paymentRepository.CreateAsync(payment);

        // ✅ Log payment creation
        await _audit.LogPaymentAsync(
            paymentId: created.Id,
            status: "Created",
            amount: created.Amount?.Amount ?? 0,
            additionalInfo: $"Payment created: {created.PaymentReference} for student {created.StudentId} - Amount: {created.Amount}"
        );

        return new PaymentDto
        {
            Id = created.Id,
            StudentId = created.StudentId,
            StudentName = created.Student?.FullName ?? "",
            Amount = created.Amount?.ToString() ?? "",
            Description = created.Description,
            Status = created.Status,
            Month = created.Month,
            Year = created.Year,
            Period = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(created.Month)} {created.Year}",
            DueDate = created.DueDate,
            PaidAt = created.PaidAt,
            IsOverdue = created.DueDate < DateTime.UtcNow && created.Status != PaymentStatus.Paid,
            CreatedAt = created.CreatedAt
        };
    }
}