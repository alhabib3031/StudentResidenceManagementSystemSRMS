using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Complaints;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// ComplaintConfiguration
/// </summary>
public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(c => c.Description)
            .HasMaxLength(2000)
            .IsRequired();
        
        builder.Property(c => c.ComplaintNumber)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(c => c.Resolution)
            .HasMaxLength(2000);
        
        // Remove static Category enum and use ComplaintType entity
        builder.HasOne(c => c.ComplaintType)
            .WithMany(ct => ct.Complaints)
            .HasForeignKey(c => c.ComplaintTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.Priority)
            .IsRequired();
        
        builder.Property(c => c.Status)
            .IsRequired();
        
        // JSON Fields
        builder.Property(c => c.AttachmentsJson)
            .HasColumnType("nvarchar(max)");
        
        builder.Property(c => c.UpdatesJson)
            .HasColumnType("nvarchar(max)");
        
        // Relationships
        // Complaint -> Reservation (Many-to-One)
        builder.HasOne(c => c.Reservation)
            .WithMany(r => r.Complaints)
            .HasForeignKey(c => c.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(c => c.ReservationId)
            .HasDatabaseName("IX_Complaints_ReservationId");
        
        builder.HasIndex(c => c.ComplaintNumber)
            .IsUnique()
            .HasDatabaseName("IX_Complaints_ComplaintNumber");
        
        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Complaints_Status");
        
        builder.HasIndex(c => c.ComplaintTypeId)
            .HasDatabaseName("IX_Complaints_ComplaintTypeId");
        
        builder.HasIndex(c => c.Priority)
            .HasDatabaseName("IX_Complaints_Priority");
        
        builder.HasIndex(c => c.AssignedTo)
            .HasDatabaseName("IX_Complaints_AssignedTo");
        
        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Complaints_CreatedAt");
        
        builder.HasIndex(c => new { c.Status, c.Priority })
            .HasDatabaseName("IX_Complaints_Status_Priority");
        
        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IX_Complaints_IsDeleted");
        
        // Query Filter
        builder.HasQueryFilter(c => !c.IsDeleted);
        
        builder.ToTable("Complaints");
    }
}
