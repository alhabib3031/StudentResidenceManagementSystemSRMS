using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Complaints;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// ComplaintTypeConfiguration
/// </summary>
public class ComplaintTypeConfiguration : IEntityTypeConfiguration<ComplaintType>
{
    public void Configure(EntityTypeBuilder<ComplaintType> builder)
    {
        builder.HasKey(ct => ct.Id);
        
        builder.Property(ct => ct.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(ct => ct.Description)
            .HasMaxLength(500);
        
        builder.HasIndex(ct => ct.Name)
            .IsUnique();
        
        builder.HasQueryFilter(ct => !ct.IsDeleted);
        
        builder.ToTable("ComplaintTypes");
    }
}
