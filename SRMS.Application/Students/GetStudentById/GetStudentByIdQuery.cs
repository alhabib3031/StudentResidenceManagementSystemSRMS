using MediatR;
using SRMS.Application.Students.DTOs;

namespace SRMS.Application.Students.GetStudentById;

public class GetStudentByIdQuery : IRequest<StudentDto>
{
    public Guid Id { get; set; }
}