using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IRepositories<Student> _studentRepository;
    
    public CreateStudentCommandHandler(IRepositories<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Map من DTO إلى Entity
        var student = request.Student.Adapt<Student>();
        student.Id = Guid.NewGuid();
        student.CreatedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        student.IsActive = true;
        student.IsDeleted = false;
        
        var createdStudent = await _studentRepository.CreateAsync(student);
        
        // Map من Entity إلى DTO للإرجاع
        return createdStudent.Adapt<StudentDto>();
    }
}