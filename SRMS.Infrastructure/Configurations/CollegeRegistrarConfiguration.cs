using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Colleges;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// CollegeRegistrarConfiguration
/// </summary>
public class CollegeRegistrarConfiguration : IEntityTypeConfiguration<CollegeRegistrar>
{
    public void Configure(EntityTypeBuilder<CollegeRegistrar> builder)
    {
        builder.HasKey(cr => cr.Id);

        // Removed FullName configuration as it's a computed property and should not be mapped to the database.
        // builder.Property(cr => cr.FullName)
        //     .HasMaxLength(200)
        //     .IsRequired();

        builder.Property(cr => cr.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(cr => cr.Email)
            .IsUnique();

        // CollegeRegistrar -> College (One-to-One - configured in CollegeConfiguration)
        // Foreign key is on CollegeRegistrar (CollegeId)

        builder.HasQueryFilter(cr => !cr.IsDeleted);

        builder.ToTable("CollegeRegistrars");
    }
}
