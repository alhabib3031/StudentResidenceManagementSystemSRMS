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
            return null;
        
        var oldStatus = existing.Status;
        var oldAmount = existing.Amount?.Amount ?? 0;
        
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
        
        
        // logs
        // If payment is being marked as paid
        if (request.Payment.Status == PaymentStatus.Paid && oldStatus != PaymentStatus.Paid)
        {
            existing.PaidAt = DateTime.UtcNow;
            existing.TransactionId = request.Payment.TransactionId;
            existing.PaymentMethod = request.Payment.PaymentMethod;
            
            // ✅ Log payment completion
            await _audit.LogPaymentAsync(
                paymentId: existing.Id,
                status: "Paid",
                amount: existing.Amount?.Amount ?? 0,
                additionalInfo: $"Payment completed - Method: {existing.PaymentMethod}, Transaction: {existing.TransactionId}"
            );
        }
        else if (request.Payment.Status == PaymentStatus.Cancelled)
        {
            // ✅ Log payment cancellation
            await _audit.LogPaymentAsync(
                paymentId: existing.Id,
                status: "Cancelled",
                amount: existing.Amount?.Amount ?? 0,
                additionalInfo: $"Payment cancelled - Reason: {request.Payment.Notes}"
            );
        }
        else if (request.Payment.Status == PaymentStatus.Refunded)
        {
            // ✅ Log payment refund
            await _audit.LogPaymentAsync(
                paymentId: existing.Id,
                status: "Refunded",
                amount: existing.Amount?.Amount ?? 0,
                additionalInfo: $"Payment refunded - Reason: {request.Payment.Notes}"
            );
        }
        else
        {
            // ✅ Log general payment update
            await _audit.LogCrudAsync<PaymentAuditChangeDto>( 
                action: AuditAction.Update,
                oldEntity: new PaymentAuditChangeDto 
                { 
                    Status = oldStatus, 
                    Amount = oldAmount 
                },
                newEntity: new PaymentAuditChangeDto 
                { 
                    Status = existing.Status, 
                    Amount = existing.Amount?.Amount ?? 0 
                },
                additionalInfo: $"Payment updated"
            );
        }
        
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