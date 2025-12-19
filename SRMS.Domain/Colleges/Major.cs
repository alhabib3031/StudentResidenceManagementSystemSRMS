using SRMS.Domain.Abstractions;

namespace SRMS.Domain.Colleges;

public class Major : Entity
{
    public string Name { get; set; } = string.Empty;
    public Guid CollegeId { get; set; }
    public College College { get; set; } = default!;
}
