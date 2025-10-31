using SRMS.Domain.Abstractions;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Payments;

public class Payment : Entity
{
    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? TransactionId { get; private set; }
    public string? PaymentMethod { get; private set; }

    public string PaymentReference { get; private set; } = string.Empty;
    public int Month { get; private set; }
    public int Year { get; private set; }
    public DateTime DueDate { get; private set; }
    public Money LateFee { get; private set; } = null!;
    public string? Notes { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid studentId,
        Money amount,
        string description,
        int month,
        int year,
        DateTime dueDate,
        string paymentReference,
        Money lateFee,
        string? paymentMethod = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException("Payment reference is required");

        return new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            Amount = amount,
            Description = description,
            Status = PaymentStatus.Pending,
            PaymentMethod = paymentMethod,
            PaymentReference = paymentReference,
            Month = month,
            Year = year,
            DueDate = dueDate,
            LateFee = lateFee,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
    }

    public void Complete(string transactionId)
    {
        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsOverdue()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as overdue");
            
        Status = PaymentStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}