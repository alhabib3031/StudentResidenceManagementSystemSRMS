using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQueryHandler : IRequestHandler<GetStudentQuery, IEnumerable<StudentDto>>
{
    private readonly IStudentRepository _studentRepository;
    
    public GetStudentQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<IEnumerable<StudentDto>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.GetAllAsync();
        
        // Map من Entity إلى DTO
        return students.Adapt<IEnumerable<StudentDto>>();
    }
}