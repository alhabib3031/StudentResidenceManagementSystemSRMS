using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.SystemSettings;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// FeesConfigurationConfiguration
/// </summary>
public class FeesConfigurationConfiguration : IEntityTypeConfiguration<FeesConfiguration>
{
    public void Configure(EntityTypeBuilder<FeesConfiguration> builder)
    {
        builder.HasKey(fc => fc.Id);

        builder.Property(fc => fc.Nationality)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fc => fc.StudyType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fc => fc.IsMonthly)
            .IsRequired();

        // Money Value Object
        builder.OwnsOne(fc => fc.FeeAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("FeeAmount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("FeeCurrency").HasMaxLength(3);
        });

        builder.Property(fc => fc.Description)
            .HasMaxLength(500);

        builder.HasIndex(fc => new { fc.Nationality, fc.StudyType })
            .IsUnique();

        builder.HasQueryFilter(fc => !fc.IsDeleted);

        builder.ToTable("FeesConfigurations");
    }
}
