using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommand : IRequest<StudentDto>
{
    public CreateStudentDto Student { get; set; } = new();
}