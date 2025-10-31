using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Managers;

namespace SRMS.Domain.Configurations;

/// <summary>
/// ManagerConfiguration
/// </summary>
public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.HasKey(m => m.Id);
        
        builder.OwnsOne(m => m.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
            
            email.HasIndex(e => e.Value).IsUnique();
        });
        
        builder.OwnsOne(m => m.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(15);
            phone.Property(p => p.CountryCode).HasColumnName("CountryCode").HasMaxLength(5);
        });
        
        builder.OwnsOne(m => m.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });
        
        builder.Property(m => m.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.LastName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.EmployeeNumber).HasMaxLength(50);
        
        builder.HasMany(m => m.Residences)
            .WithOne(r => r.Manager)
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(m => m.Students)
            .WithOne(s => s.Manager)
            .HasForeignKey(s => s.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(m => m.EmployeeNumber).IsUnique();
        builder.HasQueryFilter(m => !m.IsDeleted);
        
        builder.ToTable("Managers");
    }
}