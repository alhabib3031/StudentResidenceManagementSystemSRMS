using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs.StudentDTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudentById;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto?>
{
    private readonly IRepositories<Student> _studentRepository;
    
    public GetStudentByIdQueryHandler(IRepositories<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<StudentDto?> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        return student?.Adapt<StudentDto>();
    }
}