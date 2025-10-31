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
        
        builder.Property(p => p.PaymentReference).HasMaxLength(50).IsRequired();
        
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });
        
        builder.OwnsOne(p => p.LateFee, money =>
        {
            money.Property(m => m.Amount).HasColumnName("LateFeeAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("LateFeeCurrency").HasMaxLength(3);
        });
        
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.TransactionId).HasMaxLength(100);
        
        builder.HasOne(p => p.Student)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(p => p.PaymentReference).IsUnique();
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.DueDate);
        builder.HasIndex(p => new { p.StudentId, p.Month, p.Year });
        builder.HasQueryFilter(p => !p.IsDeleted);
        
        builder.ToTable("Payments");
    }
}