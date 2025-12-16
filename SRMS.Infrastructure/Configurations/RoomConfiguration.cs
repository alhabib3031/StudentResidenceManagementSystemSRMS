using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Rooms;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// RoomConfiguration
/// </summary>
public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.RoomNumber)
            .HasMaxLength(50)
            .IsRequired();

        // Configure Value Object Money
        builder.OwnsOne(r => r.MonthlyRent, money =>
        {
            money.Property(m => m.Amount).HasColumnName("MonthlyRent").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });
        
        builder.Property(r => r.Floor)
            .IsRequired();
        
        builder.Property(r => r.RoomType)
            .IsRequired();
        
        builder.Property(r => r.TotalBeds)
            .IsRequired();
        
        builder.Property(r => r.OccupiedBeds)
            .IsRequired();
        
        // Relationships
        builder.HasOne(r => r.Residence)
            .WithMany(res => res.Rooms)
            .HasForeignKey(r => r.ResidenceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Room -> Students (Many-to-Many via Reservation) - Configured in ReservationConfiguration
        
        // Indexes
        builder.HasIndex(r => new { r.ResidenceId, r.RoomNumber })
            .IsUnique()
            .HasDatabaseName("IX_Rooms_Residence_RoomNumber");
        
        builder.HasIndex(r => r.IsDeleted)
            .HasDatabaseName("IX_Rooms_IsDeleted");

        builder.HasIndex(r => new { r.ResidenceId, r.IsFull })
            .HasDatabaseName("IX_Rooms_Residence_IsFull");
        
        builder.Ignore(r => r.IsFull);
        
        // Query Filter
        builder.HasQueryFilter(r => !r.IsDeleted);
        
        builder.ToTable("Rooms");
    }
}