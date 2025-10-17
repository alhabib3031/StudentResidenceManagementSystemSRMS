using MediatR;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommand : IRequest<Student>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}