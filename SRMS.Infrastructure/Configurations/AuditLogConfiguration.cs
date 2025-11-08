using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.AuditLogs;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// AuditLogConfiguration
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.UserId).HasMaxLength(450);
        builder.Property(a => a.UserName).HasMaxLength(256);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(450);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.AuditAction);
        
        builder.ToTable("AuditLogs");
    }
}