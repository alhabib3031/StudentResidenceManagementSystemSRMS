using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQuery : IRequest<IEnumerable<StudentDto>>
{
    
}