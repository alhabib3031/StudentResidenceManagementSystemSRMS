using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// RoomConfiguration
/// </summary>
public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.RoomNumber).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Floor).IsRequired();
        builder.Property(r => r.RoomType).IsRequired();
        
        builder.OwnsOne(r => r.Amenities);
        
        builder.HasOne(r => r.Residence)
            .WithMany(res => res.Rooms)
            .HasForeignKey(r => r.ResidenceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(r => r.Students)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(r => new { r.ResidenceId, r.RoomNumber }).IsUnique();
        builder.HasQueryFilter(r => !r.IsDeleted);
        
        builder.ToTable("Rooms");
    }
}