using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Students;

namespace SRMS.Infrastructure.Configurations;

// <summary>
/// StudentConfiguration - تكوين Student Entity
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        
        // Value Objects
        builder.OwnsOne(s => s.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
            
            email.HasIndex(e => e.Value).IsUnique();
        });
        
        builder.OwnsOne(s => s.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(15);
            phone.Property(p => p.CountryCode).HasColumnName("CountryCode").HasMaxLength(5);
        });
        
        builder.OwnsOne(s => s.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });
        
        builder.OwnsOne(s => s.EmergencyContactPhone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("EmergencyContactPhone").HasMaxLength(15);
            phone.Property(p => p.CountryCode).HasColumnName("EmergencyContactCountryCode").HasMaxLength(5);
        });
        
        // Properties
        builder.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.LastName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.NationalId).HasMaxLength(50);
        builder.Property(s => s.StudentNumber).HasMaxLength(50);
        builder.Property(s => s.UniversityName).HasMaxLength(200);
        builder.Property(s => s.Major).HasMaxLength(100);
        
        // Relationships
        builder.HasOne(s => s.Room)
            .WithMany(r => r.Students)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(s => s.Manager)
            .WithMany(m => m.Students)
            .HasForeignKey(s => s.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Student)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(s => s.Complaints)
            .WithOne(c => c.Student)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(s => s.StudentNumber).IsUnique();
        builder.HasIndex(s => s.NationalId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.IsDeleted);
        
        // Query Filter للـ Soft Delete
        builder.HasQueryFilter(s => !s.IsDeleted);
        
        builder.ToTable("Students");
    }
}