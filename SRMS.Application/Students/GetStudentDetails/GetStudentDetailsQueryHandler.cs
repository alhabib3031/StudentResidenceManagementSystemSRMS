using MediatR;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.GetStudentDetails;

public class GetStudentDetailsQueryHandler : IRequestHandler<GetStudentDetailsQuery, StudentDetailsDto?>
{
    private readonly IRepositories<Student> _studentRepository;

    public GetStudentDetailsQueryHandler(IRepositories<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<StudentDetailsDto?> Handle(GetStudentDetailsQuery request, CancellationToken cancellationToken)
    {
        // ✅ استخدام GetByIdWithIncludesAsync من Repository
        var student = await _studentRepository.GetByIdAsync(request.Id);
        
        if (student == null)
            return null;

        return new StudentDetailsDto
        {
            Id = student.Id,
            
            // Personal Information
            FirstName = student.FirstName,
            LastName = student.LastName,
            FullName = student.FullName,
            NationalId = student.NationalId,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            ImagePath = student.ImagePath,
            
            // Contact Information
            Email = student.Email?.Value,
            PhoneNumber = student.PhoneNumber?.GetFormatted(),
            AddressCity = student.Address?.City,
            AddressStreet = student.Address?.Street,
            AddressState = student.Address?.State,
            AddressPostalCode = student.Address?.PostalCode,
            AddressCountry = student.Address?.Country,
            FullAddress = student.Address?.GetFullAddress(),
            
            // Academic Information
            UniversityName = student.UniversityName,
            StudentNumber = student.StudentNumber,
            Major = student.Major,
            AcademicYear = student.AcademicYear,
            
            // Emergency Contact
            EmergencyContactName = student.EmergencyContactName,
            EmergencyContactPhone = student.EmergencyContactPhone?.GetFormatted(),
            EmergencyContactRelation = student.EmergencyContactRelation,
            
            // Status
            Status = student.Status,
            IsActive = student.IsActive,
            
            // Audit
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            
            // Related Data Counts
            // PaymentsCount = student.Payments?.Count ?? 0,
            // ComplaintsCount = student.Complaints?.Count ?? 0
        };
    }
}