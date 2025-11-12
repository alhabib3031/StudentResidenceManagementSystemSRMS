using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Students;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// StudentConfiguration - تكوين Student Entity محسّن
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        
        // ═══════════════════════════════════════════════════════════
        // Properties Configuration
        // ═══════════════════════════════════════════════════════════
        
        builder.Property(s => s.FirstName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.LastName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.NationalId)
            .HasMaxLength(50);
        
        builder.Property(s => s.StudentNumber)
            .HasMaxLength(50);
        
        builder.Property(s => s.UniversityName)
            .HasMaxLength(200);
        
        builder.Property(s => s.Major)
            .HasMaxLength(100);
        
        builder.Property(s => s.ImagePath)
            .HasMaxLength(500);
        
        builder.Property(s => s.EmergencyContactName)
            .HasMaxLength(200);
        
        builder.Property(s => s.EmergencyContactRelation)
            .HasMaxLength(100);
        
        // ═══════════════════════════════════════════════════════════
        // Value Objects Configuration
        // ═══════════════════════════════════════════════════════════
        
        // Email Value Object
        builder.OwnsOne(s => s.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired(false);
            
            email.Ignore(e => e.Value); // لأننا نستخدم الـ backing field
            
            // Index على Email للبحث السريع
            email.HasIndex(e => e.Value)
                .HasDatabaseName("IX_Students_Email");
        });
        
        // PhoneNumber Value Object
        builder.OwnsOne(s => s.PhoneNumber, phone =>
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
        
        // Emergency Contact Phone Value Object
        builder.OwnsOne(s => s.EmergencyContactPhone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("EmergencyContactPhone")
                .HasMaxLength(15)
                .IsRequired(false);
            
            phone.Property(p => p.CountryCode)
                .HasColumnName("EmergencyContactPhoneCountryCode")
                .HasMaxLength(5)
                .IsRequired(false);
        });
        
        // Address Value Object
        builder.OwnsOne(s => s.Address, address =>
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
        
        // Student -> Room (Many-to-One)
        builder.HasOne(s => s.Room)
            .WithMany(r => r.Students)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Student -> Manager (Many-to-One)
        builder.HasOne(s => s.Manager)
            .WithMany(m => m.Students)
            .HasForeignKey(s => s.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Student -> Payments (One-to-Many)
        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Student)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Student -> Complaints (One-to-Many)
        builder.HasMany(s => s.Complaints)
            .WithOne(c => c.Student)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ═══════════════════════════════════════════════════════════
        // Indexes
        // ═══════════════════════════════════════════════════════════
        
        builder.HasIndex(s => s.StudentNumber)
            .IsUnique()
            .HasDatabaseName("IX_Students_StudentNumber")
            .HasFilter("[StudentNumber] IS NOT NULL");
        
        builder.HasIndex(s => s.NationalId)
            .HasDatabaseName("IX_Students_NationalId")
            .HasFilter("[NationalId] IS NOT NULL");
        
        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_Students_Status");
        
        builder.HasIndex(s => s.IsDeleted)
            .HasDatabaseName("IX_Students_IsDeleted");
        
        builder.HasIndex(s => s.CreatedAt)
            .HasDatabaseName("IX_Students_CreatedAt");
        
        // Composite Index للبحث المتقدم
        builder.HasIndex(s => new { s.Status, s.IsActive, s.IsDeleted })
            .HasDatabaseName("IX_Students_Status_Active_Deleted");
        
        // ═══════════════════════════════════════════════════════════
        // Query Filter للـ Soft Delete
        // ═══════════════════════════════════════════════════════════
        
        builder.HasQueryFilter(s => !s.IsDeleted);
        
        // ═══════════════════════════════════════════════════════════
        // Table Configuration
        // ═══════════════════════════════════════════════════════════
        
        builder.ToTable("Students");
    }
}
