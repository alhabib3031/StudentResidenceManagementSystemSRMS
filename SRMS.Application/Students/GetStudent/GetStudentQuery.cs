using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Application.Students.DTOs.StudentDTOs;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQuery : IRequest<IEnumerable<StudentDto>>
{
    
}