using Mapster;
using MapsterMapper;
using MediatR;
using SRMS.Application.Students.DTOs.StudentDTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IRepositories<Student> _studentRepository;
    private readonly IMapper _mapper;
    
    public CreateStudentCommandHandler(IRepositories<Student> studentRepository, IMapper mapper)
    {
        _studentRepository = studentRepository;
        _mapper = mapper;
    }
    
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Map من DTO إلى Entity
        // var student = request.Student.Adapt<Student>(); // هذا الاصدار الاول
        var student = _mapper.Map<Student>(request.Student);
        
        student.Id = Guid.NewGuid();
        student.CreatedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        student.IsActive = true;
        student.IsDeleted = false;
        
        var createdStudent = await _studentRepository.CreateAsync(student);
        
        // Map من Entity إلى DTO للإرجاع
        // return createdStudent.Adapt<StudentDto>(); // هذا الاصدار الاول 
        return _mapper.Map<StudentDto>(createdStudent);
    }
}