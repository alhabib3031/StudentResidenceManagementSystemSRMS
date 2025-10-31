using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Complaints;

namespace SRMS.Domain.Configurations;

/// <summary>
/// ComplaintConfiguration
/// </summary>
public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.ComplaintNumber).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.Resolution).HasMaxLength(1000);
        
        builder.HasOne(c => c.Student)
            .WithMany(s => s.Complaints)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ComplaintUpdates as owned entity
        builder.OwnsMany(c => c.Updates, update =>
        {
            update.WithOwner().HasForeignKey("ComplaintId");
            update.Property(u => u.Message).HasMaxLength(500);
            update.ToTable("ComplaintUpdates");
        });
        
        builder.HasIndex(c => c.ComplaintNumber).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Category);
        builder.HasIndex(c => c.Priority);
        builder.HasQueryFilter(c => !c.IsDeleted);
        
        builder.ToTable("Complaints");
    }
}