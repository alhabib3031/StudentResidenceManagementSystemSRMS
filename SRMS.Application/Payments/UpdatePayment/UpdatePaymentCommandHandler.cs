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
    private readonly IAuditService _audit;

    public UpdatePaymentCommandHandler(IRepositories<Payment> paymentRepository, IAuditService audit)
    {
        _paymentRepository = paymentRepository;
        _audit = audit;
    }

    public async Task<PaymentDto?> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _paymentRepository.GetByIdAsync(request.Payment.Id);

        if (existing == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Payment",
                request.Payment.Id.ToString(),
                additionalInfo: "Attempted to update non-existent payment"
            );
            return null;
        }

        var oldStatus = existing.Status;
        var oldAmount = existing.Amount?.Amount ?? 0;
        var oldValues = new
        {
            existing.Status,
            Amount = existing.Amount?.Amount,
            existing.Description,
            existing.TransactionId,
            existing.PaymentMethod
        };

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
            await _audit.LogAsync(
                AuditAction.Error,
                "Payment",
                existing.Id.ToString(),
                additionalInfo: $"Invalid amount during payment update: {ex.Message}"
            );
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
                await _audit.LogAsync(
                    AuditAction.Error,
                    "Payment",
                    existing.Id.ToString(),
                    additionalInfo: $"Invalid late fee during payment update: {ex.Message}"
                );
                throw new InvalidOperationException($"Invalid late fee: {ex.Message}");
            }
        }
        else
        {
            existing.LateFee = null;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _paymentRepository.UpdateAsync(existing);

        // Store new values
        var newValues = new
        {
            updated.Status,
            Amount = updated.Amount?.Amount,
            updated.Description,
            updated.TransactionId,
            updated.PaymentMethod
        };


        // ✅ Log payment update based on status change
        if (oldStatus != updated.Status)
        {
            switch (updated.Status)
            {
                case PaymentStatus.Paid:
                    await _audit.LogPaymentAsync(
                        paymentId: updated.Id,
                        status: "Paid",
                        amount: updated.Amount?.Amount ?? 0,
                        additionalInfo: $"Payment completed - Reference: {updated.PaymentReference}, Method: {updated.PaymentMethod}, Transaction: {updated.TransactionId}"
                    );
                    break;

                case PaymentStatus.Cancelled:
                    await _audit.LogPaymentAsync(
                        paymentId: updated.Id,
                        status: "Cancelled",
                        amount: updated.Amount?.Amount ?? 0,
                        additionalInfo: $"Payment cancelled - Reference: {updated.PaymentReference}, Reason: {updated.Notes}"
                    );
                    break;

                case PaymentStatus.Refunded:
                    await _audit.LogPaymentAsync(
                        paymentId: updated.Id,
                        status: "Refunded",
                        amount: updated.Amount?.Amount ?? 0,
                        additionalInfo: $"Payment refunded - Reference: {updated.PaymentReference}, Reason: {updated.Notes}"
                    );
                    break;

                case PaymentStatus.Overdue:
                    await _audit.LogPaymentAsync(
                        paymentId: updated.Id,
                        status: "Overdue",
                        amount: updated.Amount?.Amount ?? 0,
                        additionalInfo: $"Payment marked as overdue - Reference: {updated.PaymentReference}"
                    );
                    break;

                default:
                    await _audit.LogCrudAsync(
                        action: AuditAction.Update,
                        oldEntity: oldValues,
                        newEntity: newValues,
                        additionalInfo: $"Payment updated: {updated.PaymentReference}"
                    );
                    break;
            }
        }
        else
        {
            // General update log
            await _audit.LogCrudAsync(
                action: AuditAction.Update,
                oldEntity: oldValues,
                newEntity: newValues,
                additionalInfo: $"Payment updated: {updated.PaymentReference}"
            );
        }

        return new PaymentDto
        {
            Id = updated.Id,
            ReservationId = updated.ReservationId.Value,
            StudentName = updated.Reservation?.Student?.FullName ?? "",
            Amount = updated.Amount?.Amount ?? 0m,
            Description = updated.Description,
            Status = updated.Status,
            Month = updated.Month,
            Year = updated.Year,
            Period = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(updated.Month)} {updated.Year}",
            DueDate = updated.DueDate,
            PaidAt = updated.PaidAt,
            CreatedAt = updated.CreatedAt
        };
    }
}