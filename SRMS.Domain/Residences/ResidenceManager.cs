using SRMS.Domain.Abstractions;
using SRMS.Domain.Managers;

namespace SRMS.Domain.Residences;

/// <summary>
/// Intermediate entity for the Many-to-Many relationship between Residence and Manager.
/// </summary>
public class ResidenceManager : Entity
{
    public Guid ResidenceId { get; set; }
    public Residence Residence { get; set; } = null!;

    public Guid ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;

    // Additional properties for the relationship, if needed (e.g., AssignmentDate, IsPrimaryManager)
    public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
}
