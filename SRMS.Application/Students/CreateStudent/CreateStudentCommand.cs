using MediatR;
using SRMS.Application.Students.DTOs.StudentDTOs;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommand : IRequest<StudentDto>
{
    public CreateStudentDto Student { get; set; } = new();
}