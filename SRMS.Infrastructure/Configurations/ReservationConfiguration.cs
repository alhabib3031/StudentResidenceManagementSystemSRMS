using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Reservations;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// ReservationConfiguration - M:N relationship between Student and Room
/// </summary>
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        // Student -> Reservation (One-to-Many)
        builder.HasOne(r => r.Student)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Room -> Reservation (One-to-Many)
        builder.HasOne(r => r.Room)
            .WithMany(rm => rm.Reservations)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a room if it has active reservations

        // Reservation -> Complaints (One-to-Many)
        builder.HasMany(r => r.Complaints)
            .WithOne(c => c.Reservation)
            .HasForeignKey(c => c.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Reservations");
    }
}
