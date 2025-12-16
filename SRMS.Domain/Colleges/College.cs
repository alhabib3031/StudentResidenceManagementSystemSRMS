using SRMS.Domain.Abstractions;
using SRMS.Domain.Colleges.Enums;
using SRMS.Domain.Students;

namespace SRMS.Domain.Colleges;

/// <summary>
/// College Entity - الكلية
/// </summary>
public class College : Entity
{
    public string Name { get; set; } = string.Empty;
    public StudySystem StudySystem { get; set; }
    
    // Future properties can be added here
    
    // Navigation Properties
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public CollegeRegistrar? CollegeRegistrar { get; set; }

}
