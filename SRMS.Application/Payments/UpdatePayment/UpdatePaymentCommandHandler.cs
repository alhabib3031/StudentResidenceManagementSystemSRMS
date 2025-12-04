using System.Globalization;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Payments.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Payments;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Payments.UpdatePayment;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, PaymentDto?>
{
    private readonly IRepositories<Payment> _paymentRepository;

    public UpdatePaymentCommandHandler(IRepositories<Payment> paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _paymentRepository.GetByIdAsync(request.Payment.Id);

        if (existing is null) return null;

        // Update properties
        existing.Description = request.Payment.Description;
        existing.Status = request.Payment.Status;
        existing.PaidAt = request.Payment.PaidAt;
        existing.TransactionId = request.Payment.TransactionId;
        existing.PaymentMethod = request.Payment.PaymentMethod;
        existing.Notes = request.Payment.Notes;

        // Amount (Value Object)
        try
        {
            existing.Amount = Money.Create(request.Payment.Amount, request.Payment.Currency);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Invalid amount: {ex.Message}");
        }

        // Late Fee (Value Object)
        if (request.Payment.LateFeeAmount.HasValue && request.Payment.LateFeeAmount.Value > 0)
        {
            try
            {
                existing.LateFee = Money.Create(
                    request.Payment.LateFeeAmount.Value,
                    request.Payment.LateFeeСurrency ?? "LYD"
                );
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"Invalid late fee: {ex.Message}");
            }
        }
        else
        {
            existing.LateFee = null;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _paymentRepository.UpdateAsync(existing);

        return new PaymentDto
        {
            Id = updated.Id,
            StudentId = updated.StudentId,
            StudentName = updated.Student?.FullName ?? "",
            Amount = updated.Amount?.ToString() ?? "",
            Description = updated.Description,
            Status = updated.Status,
            Month = updated.Month,
            Year = updated.Year,
            Period = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(updated.Month)} {updated.Year}",
            DueDate = updated.DueDate,
            PaidAt = updated.PaidAt,
            IsOverdue = updated.DueDate < DateTime.UtcNow && updated.Status != PaymentStatus.Paid,
            CreatedAt = updated.CreatedAt
        };
    }
}