using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Managers;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// ManagerConfiguration - تكوين Manager Entity محسّن
/// </summary>
public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.HasKey(m => m.Id);
        
        // ═══════════════════════════════════════════════════════════
        // Properties Configuration
        // ═══════════════════════════════════════════════════════════
        
        builder.Property(m => m.FirstName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(m => m.LastName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(m => m.EmployeeNumber)
            .HasMaxLength(50);
        
        builder.Property(m => m.ImagePath)
            .HasMaxLength(500);
        
        // ═══════════════════════════════════════════════════════════
        // Value Objects Configuration
        // ═══════════════════════════════════════════════════════════
        
        // Email Value Object
        builder.OwnsOne(m => m.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired(false);
            
            // Index على Email للبحث السريع
            email.HasIndex(e => e.Value)
                .HasDatabaseName("IX_Managers_Email")
                .HasFilter("[Email] IS NOT NULL");
        });
        
        // PhoneNumber Value Object
        builder.OwnsOne(m => m.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(15)
                .IsRequired(false);
            
            phone.Property(p => p.CountryCode)
                .HasColumnName("PhoneCountryCode")
                .HasMaxLength(5)
                .IsRequired(false);
        });
        
        // Address Value Object
        builder.OwnsOne(m => m.Address, address =>
        {
            address.Property(a => a.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(100)
                .IsRequired(false);
            
            address.Property(a => a.Street)
                .HasColumnName("AddressStreet")
                .HasMaxLength(200)
                .IsRequired(false);
            
            address.Property(a => a.State)
                .HasColumnName("AddressState")
                .HasMaxLength(100)
                .IsRequired(false);
            
            address.Property(a => a.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(20)
                .IsRequired(false);
            
            address.Property(a => a.Country)
                .HasColumnName("AddressCountry")
                .HasMaxLength(100)
                .IsRequired(false);
        });
        
        // ═══════════════════════════════════════════════════════════
        // Relationships Configuration
        // ═══════════════════════════════════════════════════════════
        
        // Manager -> Residences (Many-to-Many via ResidenceManager) - Configured in ResidenceManagerConfiguration
        // Manager -> Students (Link removed as per re-engineering request)
        
        // ═══════════════════════════════════════════════════════════
        // Indexes
        // ═══════════════════════════════════════════════════════════
        
        builder.HasIndex(m => m.EmployeeNumber)
            .IsUnique()
            .HasDatabaseName("IX_Managers_EmployeeNumber")
            .HasFilter("[EmployeeNumber] IS NOT NULL");
        
        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_Managers_Status");
        
        builder.HasIndex(m => m.IsDeleted)
            .HasDatabaseName("IX_Managers_IsDeleted");
        
        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_Managers_CreatedAt");
        
        // Composite Index
        builder.HasIndex(m => new { m.Status, m.IsActive, m.IsDeleted })
            .HasDatabaseName("IX_Managers_Status_Active_Deleted");
        
        // ═══════════════════════════════════════════════════════════
        // Query Filter للـ Soft Delete
        // ═══════════════════════════════════════════════════════════
        
        builder.HasQueryFilter(m => !m.IsDeleted);
        
        // ═══════════════════════════════════════════════════════════
        // Table Configuration
        // ═══════════════════════════════════════════════════════════
        
        builder.ToTable("Managers");
    }
}