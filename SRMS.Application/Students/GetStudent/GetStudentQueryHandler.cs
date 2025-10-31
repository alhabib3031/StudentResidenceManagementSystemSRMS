using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs.StudentDTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudent;

public class GetStudentQueryHandler : IRequestHandler<GetStudentQuery, IEnumerable<StudentDto>>
{
    private readonly IRepositories<Student> _studentRepository;
    
    public GetStudentQueryHandler(IRepositories<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<IEnumerable<StudentDto>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.GetAllAsync();

        // مثال: إذا المستخدم أغلق الصفحة قبل انتهاء العملية
        return cancellationToken.IsCancellationRequested ?
            // أوقف العملية
            null! :
            // Map من Entity إلى DTO
            students.Adapt<IEnumerable<StudentDto>>();
    }
}