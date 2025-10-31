using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs.StudentDTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

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
        
        // Properly convert string values to value objects
        existingStudent.Email = !string.IsNullOrEmpty(request.Student.Email)
            ? Email.Create(request.Student.Email)
            : null;
        
        existingStudent.PhoneNumber = !string.IsNullOrEmpty(request.Student.PhoneNumber)
            ? PhoneNumber.Create(request.Student.PhoneNumber)
            : null;
        
        if (existingStudent.Address != null)
            existingStudent.Address = !string.IsNullOrEmpty(request.Student.Address)
                ? Address.Create(
                    existingStudent.Address.City,
                    existingStudent.Address.Street,
                    existingStudent.Address.State,
                    existingStudent.Address.PostalCode,
                    existingStudent.Address.Country
                    )
                : null ;
        
        existingStudent.UpdatedAt = DateTime.UtcNow;
        
        var updatedStudent = await _studentRepository.UpdateAsync(existingStudent);
        return updatedStudent.Adapt<StudentDto>();
    }
}