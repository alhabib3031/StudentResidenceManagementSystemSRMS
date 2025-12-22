using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Common;

namespace SRMS.Infrastructure.Configurations;

public class NationalityConfiguration : IEntityTypeConfiguration<Nationality>
{
    public void Configure(EntityTypeBuilder<Nationality> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Name).HasMaxLength(100).IsRequired();
        builder.Property(n => n.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(n => n.CountryCode).HasMaxLength(3).IsRequired();

        builder.HasIndex(n => n.Name).IsUnique();
        builder.HasIndex(n => n.CountryCode).IsUnique();

        builder.HasQueryFilter(n => !n.IsDeleted);

        builder.ToTable("Nationalities");
    }
}
