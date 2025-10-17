using MediatR;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Student>
{
    private readonly IStudentRepository _studentRepository;
    
    public CreateStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<Student> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
        
        return await _studentRepository.CreateAsync(student);
    }
}