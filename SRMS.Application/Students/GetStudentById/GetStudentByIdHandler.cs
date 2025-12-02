using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs;
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
        
        if (student == null)
            return null;

        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            FullName = student.FullName,
            Email = student.Email?.Value,
            PhoneNumber = student.PhoneNumber?.GetFormatted(),
            Address = student.Address?.GetFullAddress(),
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            IsActive = student.IsActive,
            Status = student.Status
        };
    }
}