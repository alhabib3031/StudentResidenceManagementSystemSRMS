using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRMS.Domain.Residences;

namespace SRMS.Infrastructure.Configurations;

/// <summary>
/// ResidenceManagerConfiguration - M:N relationship between Residence and Manager
/// </summary>
public class ResidenceManagerConfiguration : IEntityTypeConfiguration<ResidenceManager>
{
    public void Configure(EntityTypeBuilder<ResidenceManager> builder)
    {
        builder.HasKey(rm => rm.Id);

        // Composite unique index
        builder.HasIndex(rm => new { rm.ResidenceId, rm.ManagerId })
            .IsUnique();

        // Residence -> ResidenceManager (One-to-Many)
        builder.HasOne(rm => rm.Residence)
            .WithMany(r => r.ResidenceManagers)
            .HasForeignKey(rm => rm.ResidenceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Manager -> ResidenceManager (One-to-Many)
        builder.HasOne(rm => rm.Manager)
            .WithMany(m => m.ResidenceManagers)
            .HasForeignKey(rm => rm.ManagerId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a manager if they are assigned to a residence

        builder.ToTable("ResidenceManagers");
    }
}
