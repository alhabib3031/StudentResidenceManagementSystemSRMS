using SRMS.Domain.Abstractions;

namespace SRMS.Domain.Complaints;

/// <summary>
/// Complaint Type Entity - نوع الشكوى (كيان مرن)
/// </summary>
public class ComplaintType : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
