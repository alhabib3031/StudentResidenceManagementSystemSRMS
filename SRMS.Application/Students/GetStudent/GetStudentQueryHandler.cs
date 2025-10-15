using MediatR;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQueryHandler : IRequestHandler<GetStudentQuery, IEnumerable<Student>>
{
    private readonly IStudentRepository _studentRepository;
    
    public GetStudentQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<IEnumerable<Student>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.GetAllAsync();
        return students;
    }
}