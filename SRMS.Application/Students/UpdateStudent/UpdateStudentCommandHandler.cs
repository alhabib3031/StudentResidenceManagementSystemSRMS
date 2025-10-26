using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, StudentDto?>
{
    private readonly IRepositories<Student> _studentRepository;
    
    public UpdateStudentCommandHandler(IRepositories<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<StudentDto?> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var existingStudent = await _studentRepository.GetByIdAsync(request.Student.Id);
        
        if (existingStudent == null)
            return null!;
        
        // Update properties
        existingStudent.FirstName = request.Student.FirstName;
        existingStudent.LastName = request.Student.LastName;
        existingStudent.Email = request.Student.Email;
        existingStudent.PhoneNumber = request.Student.PhoneNumber;
        existingStudent.Address = request.Student.Address;
        existingStudent.UpdatedAt = DateTime.UtcNow;
        
        var updatedStudent = await _studentRepository.UpdateAsync(existingStudent);
        return updatedStudent.Adapt<StudentDto>();
    }
}