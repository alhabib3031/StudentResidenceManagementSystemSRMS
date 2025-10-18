using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudentById;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto?>
{
    private readonly IStudentRepository _studentRepository;
    
    public GetStudentByIdQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<StudentDto?> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        return student?.Adapt<StudentDto>();
    }
}