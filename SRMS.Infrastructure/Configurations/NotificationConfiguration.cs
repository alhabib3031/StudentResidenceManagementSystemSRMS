using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Notifications;

namespace SRMS.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();
        
        builder.Property(n => n.UserEmail)
            .HasMaxLength(256);
        
        builder.Property(n => n.UserPhone)
            .HasMaxLength(20);
        
        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(100);
        
        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);
        
        builder.Property(n => n.MetadataJson)
            .HasColumnType("nvarchar(max)");
        
        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);
        
        // Indexes
        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("IX_Notifications_UserId");
        
        builder.HasIndex(n => n.Status)
            .HasDatabaseName("IX_Notifications_Status");
        
        builder.HasIndex(n => n.IsRead)
            .HasDatabaseName("IX_Notifications_IsRead");
        
        builder.HasIndex(n => n.Type)
            .HasDatabaseName("IX_Notifications_Type");
        
        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notifications_User_Read");
        
        builder.HasIndex(n => new { n.Status, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_Status_Created");
        
        builder.HasIndex(n => n.IsDeleted)
            .HasDatabaseName("IX_Notifications_IsDeleted");
        
        // Query Filter
        builder.HasQueryFilter(n => !n.IsDeleted);
        
        builder.ToTable("Notifications");
    }
}