using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Payments;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// PaymentConfiguration
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(p => p.PaymentReference)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);
        
        builder.Property(p => p.PaymentMethod)
            .HasMaxLength(50);
        
        builder.Property(p => p.Notes)
            .HasMaxLength(1000);
        
        // Amount (Value Object)
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        // Late Fee (Value Object)
        builder.OwnsOne(p => p.LateFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("LateFeeAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);
            
            money.Property(m => m.Currency)
                .HasColumnName("LateFeeCurrency")
                .HasMaxLength(3)
                .IsRequired(false);
        });
        
        // Relationships
        builder.HasOne(p => p.Reservation)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(p => p.ReservationId)
            .HasDatabaseName("IX_Payments_ReservationId");
        
        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payments_Status");
        
        builder.HasIndex(p => p.DueDate)
            .HasDatabaseName("IX_Payments_DueDate");
        
        builder.HasIndex(p => new { p.ReservationId, p.Month, p.Year })
            .IsUnique()
            .HasDatabaseName("IX_Payments_Reservation_Period");
        
        builder.HasIndex(p => p.PaymentReference)
            .IsUnique()
            .HasDatabaseName("IX_Payments_Reference");
        
        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Payments_IsDeleted");
        
        // Query Filter
        builder.HasQueryFilter(p => !p.IsDeleted);
        
        builder.ToTable("Payments");
    }
}