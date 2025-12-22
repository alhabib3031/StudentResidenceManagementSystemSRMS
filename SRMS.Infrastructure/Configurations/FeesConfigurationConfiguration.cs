using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.SystemSettings;
using SRMS.Domain.Common;

namespace SRMS.Infrastructure.Configurations;

public class FeesConfigurationConfiguration : IEntityTypeConfiguration<FeesConfiguration>
{
    public void Configure(EntityTypeBuilder<FeesConfiguration> builder)
    {
        builder.HasKey(fc => fc.Id);

        builder.Property(fc => fc.StudyLevel)
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

        builder.HasOne(fc => fc.Nationality)
            .WithMany()
            .HasForeignKey(fc => fc.NationalityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on Nationality + StudyLevel
        builder.HasIndex(fc => new { fc.NationalityId, fc.StudyLevel })
            .IsUnique();

        builder.HasQueryFilter(fc => !fc.IsDeleted);

        builder.ToTable("FeesConfigurations");
    }
}
