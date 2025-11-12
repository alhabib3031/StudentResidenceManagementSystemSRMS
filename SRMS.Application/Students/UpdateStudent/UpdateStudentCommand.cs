using MediatR;
using SRMS.Application.Students.DTOs;

namespace SRMS.Application.Students.UpdateStudent;

public class UpdateStudentCommand : IRequest<StudentDto?>
{
    public UpdateStudentDto Student { get; set; } = new();
}