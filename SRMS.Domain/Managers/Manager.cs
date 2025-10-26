using SRMS.Domain.Abstractions;
using SRMS.Domain.Students;

namespace SRMS.Domain.Managers;

public class Manager : Entity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ImagePath { get; set; }
    
    public ICollection<Student> Students { get; set; } = new List<Student>();
}