using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Colleges;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// CollegeConfiguration
/// </summary>
public class CollegeConfiguration : IEntityTypeConfiguration<College>
{
    public void Configure(EntityTypeBuilder<College> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.StudySystem)
            .IsRequired();

        // College -> CollegeRegistrar (One-to-One)
        builder.HasOne(c => c.CollegeRegistrar)
            .WithOne(cr => cr.College)
            .HasForeignKey<CollegeRegistrar>(cr => cr.CollegeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.ToTable("Colleges");
    }
}
