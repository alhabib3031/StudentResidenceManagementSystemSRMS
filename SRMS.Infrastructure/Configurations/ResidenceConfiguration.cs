using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Residences;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// ResidenceConfiguration
/// </summary>
public class ResidenceConfiguration : IEntityTypeConfiguration<Residence>
{
    public void Configure(EntityTypeBuilder<Residence> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.OwnsOne(r => r.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });
        
        builder.OwnsOne(r => r.MonthlyRent, money =>
        {
            money.Property(m => m.Amount).HasColumnName("MonthlyRent").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });
        
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(1000);
        
        builder.HasOne(r => r.Manager)
            .WithMany(m => m.Residences)
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(r => r.Rooms)
            .WithOne(rm => rm.Residence)
            .HasForeignKey(rm => rm.ResidenceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(r => r.Name);
        builder.HasQueryFilter(r => !r.IsDeleted);
        
        builder.ToTable("Residences");
    }
}