using System.Diagnostics;
using Mapster;
using MapsterMapper;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Students.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IRepositories<Student> _studentRepository;
    private readonly IAuditService _audit;

    public CreateStudentCommandHandler(IRepositories<Student> studentRepository, IAuditService audit)
    {
        _studentRepository = studentRepository;
        _audit = audit;
    }

    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // ✅ إنشاء Student جديد
        Debug.Assert(request.Student.AcademicYear != null, "request.Student.AcademicYear != null");
        
        var student = new Student
        {
            Id = Guid.NewGuid(),

            // Personal Information
            FirstName = request.Student.FirstName,
            LastName = request.Student.LastName,
            NationalId = request.Student.NationalId,
            DateOfBirth = request.Student.DateOfBirth,
            Gender = request.Student.Gender,

            // Academic Information
            UniversityName = request.Student.UniversityName,
            StudentNumber = request.Student.StudentNumber,
            Major = request.Student.Major,
            AcademicYear = request.Student.AcademicYear.Value,

            // Emergency Contact
            EmergencyContactName = request.Student.EmergencyContactName,
            EmergencyContactRelation = request.Student.EmergencyContactRelation,

            // Audit
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };

        // ✅ إنشاء Email (Value Object)
        try
        {
            student.Email = !string.IsNullOrWhiteSpace(request.Student.Email)
                ? Email.Create(request.Student.Email)
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Student",
                additionalInfo: $"Invalid email during student creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid email: {ex.Message}");
        }

        // ✅ إنشاء PhoneNumber (Value Object)
        try
        {
            student.PhoneNumber = !string.IsNullOrWhiteSpace(request.Student.PhoneNumber)
                ? PhoneNumber.Create(
                    request.Student.PhoneNumber,
                    request.Student.PhoneCountryCode ?? "+218")
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Student",
                additionalInfo: $"Invalid phone number during student creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid phone number: {ex.Message}");
        }

        // ✅ إنشاء Emergency Contact Phone (Value Object)
        try
        {
            student.EmergencyContactPhone = !string.IsNullOrWhiteSpace(request.Student.EmergencyContactPhone)
                ? PhoneNumber.Create(
                    request.Student.EmergencyContactPhone,
                    request.Student.EmergencyContactPhoneCountryCode ?? "+218")
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Student",
                additionalInfo: $"Invalid emergency contact phone during student creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid emergency contact phone: {ex.Message}");
        }

        // ✅ إنشاء Address (Value Object)
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Student.AddressCity))
            {
                student.Address = Address.Create(
                    city: request.Student.AddressCity,
                    street: request.Student.AddressStreet ?? "",
                    state: request.Student.AddressState ?? "",
                    postalCode: request.Student.AddressPostalCode ?? "",
                    country: request.Student.AddressCountry ?? "Libya"
                );
            }
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Student",
                additionalInfo: $"Invalid address during student creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid address: {ex.Message}");
        }

        // ✅ حفظ في قاعدة البيانات
        var created = await _studentRepository.CreateAsync(student);

        // ✅ Log student registration
        await _audit.LogCrudAsync(
            action: AuditAction.StudentRegistered,
            newEntity: new
            {
                created.Id,
                created.FullName,
                created.StudentNumber,
                Email = created.Email?.Value,
                created.UniversityName,
                created.Major,
                created.Status
            },
            additionalInfo: $"New student registered: {created.FullName} (Student #: {created.StudentNumber})"
        );

        // ✅ إرجاع DTO
        return new StudentDto
        {
            Id = created.Id,
            FirstName = created.FirstName,
            LastName = created.LastName,
            FullName = created.FullName,
            Email = created.Email?.Value,
            PhoneNumber = created.PhoneNumber?.GetFormatted(),
            Address = created.Address?.GetFullAddress(),
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt,
            IsActive = created.IsActive,
            Status = created.Status
        };
    }
}