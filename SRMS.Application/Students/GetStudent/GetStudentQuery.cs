using MediatR;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQuery : IRequest<IEnumerable<Student>>
{
    
}