using MediatR;
using SRMS.Application.Students.DTOs;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQuery : IRequest<IEnumerable<StudentDto>>
{
    
}