using Mapster;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Students.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, StudentDto?>
{
    private readonly IRepositories<Student> _studentRepository;
    private readonly IAuditService _audit;

    public UpdateStudentCommandHandler(IRepositories<Student> studentRepository, IAuditService audit)
    {
        _studentRepository = studentRepository;
        _audit = audit;
    }

    public async Task<StudentDto?> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _studentRepository.GetByIdAsync(request.Student.Id);

        if (existing == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Student",
                request.Student.Id.ToString(),
                additionalInfo: "Attempted to update non-existent student"
            );
            return null;
        }

        // ✅ Store old values for audit
        var oldValues = new
        {
            existing.FirstName,
            existing.LastName,
            existing.NationalId,
            existing.DateOfBirth,
            existing.Gender,
            Email = existing.Email?.Value,
            PhoneNumber = existing.PhoneNumber?.GetFormatted(),
            existing.UniversityName,
            existing.StudentNumber,
            existing.Major,
            existing.AcademicYear,
            existing.Status,
        };

        // ✅ تحديث الخصائص البسيطة
        existing.FirstName = request.Student.FirstName;
        existing.LastName = request.Student.LastName;
        existing.NationalId = request.Student.NationalId;
        existing.NationalityId = request.Student.NationalityId;
        existing.StudyLevel = request.Student.StudyLevel;
        existing.DateOfBirth = request.Student.DateOfBirth;
        existing.Gender = request.Student.Gender;

        // ✅ تحديث المعلومات الأكاديمية
        existing.UniversityName = request.Student.UniversityName;
        existing.StudentNumber = request.Student.StudentNumber;
        existing.Major = request.Student.Major;
        if (request.Student.AcademicYear != null) existing.AcademicYear = request.Student.AcademicYear.Value;

        // ✅ تحديث معلومات الطوارئ
        existing.EmergencyContactName = request.Student.EmergencyContactName;
        existing.EmergencyContactRelation = request.Student.EmergencyContactRelation;

        // ✅ تحديث الحالة
        existing.Status = request.Student.Status;

        // ✅ تحديث Email (Value Object)
        try
        {
            existing.Email = !string.IsNullOrWhiteSpace(request.Student.Email)
                ? Email.Create(request.Student.Email)
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Student",
                existing.Id.ToString(),
                additionalInfo: $"Invalid email during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid email: {ex.Message}");
        }

        // ✅ تحديث PhoneNumber (Value Object)
        try
        {
            existing.PhoneNumber = !string.IsNullOrWhiteSpace(request.Student.PhoneNumber)
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
                existing.Id.ToString(),
                additionalInfo: $"Invalid phone number during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid phone number: {ex.Message}");
        }

        // ✅ تحديث Emergency Contact Phone (Value Object)
        try
        {
            existing.EmergencyContactPhone = !string.IsNullOrWhiteSpace(request.Student.EmergencyContactPhone)
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
                existing.Id.ToString(),
                additionalInfo: $"Invalid emergency contact phone during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid emergency contact phone: {ex.Message}");
        }

        // ✅ تحديث Address (Value Object) - بالقيم الجديدة من DTO
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Student.AddressCity))
            {
                existing.Address = Address.Create(
                    city: request.Student.AddressCity,
                    street: request.Student.AddressStreet ?? "",
                    state: request.Student.AddressState ?? "",
                    postalCode: request.Student.AddressPostalCode ?? "",
                    country: request.Student.AddressCountry ?? "Libya"
                );
            }
            else
            {
                existing.Address = null;
            }
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Student",
                existing.Id.ToString(),
                additionalInfo: $"Invalid address during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid address: {ex.Message}");
        }

        // ✅ تحديث Audit fields
        existing.UpdatedAt = DateTime.UtcNow;

        // ✅ حفظ التغييرات
        var updated = await _studentRepository.UpdateAsync(existing);

        // ✅ Store new values for audit
        var newValues = new
        {
            updated.FirstName,
            updated.LastName,
            updated.NationalId,
            updated.DateOfBirth,
            updated.Gender,
            Email = updated.Email?.Value,
            PhoneNumber = updated.PhoneNumber?.GetFormatted(),
            updated.UniversityName,
            updated.StudentNumber,
            updated.Major,
            updated.AcademicYear,
            updated.Status,
        };

        // ✅ Log student update
        await _audit.LogCrudAsync(
            action: AuditAction.Update,
            oldEntity: oldValues,
            newEntity: newValues,
            additionalInfo: $"Student updated: {updated.FullName}"
        );

        // ✅ If status changed, log status change
        if (oldValues.Status != newValues.Status)
        {
            await _audit.LogStudentStatusChangeAsync(
                studentId: updated.Id,
                oldStatus: oldValues.Status.ToString(),
                newStatus: newValues.Status.ToString(),
                reason: "Status changed via update"
            );
        }

        // ✅ Room assignment logic removed as per re-engineering

        // ✅ إرجاع DTO
        return new StudentDto
        {
            Id = updated.Id,
            FirstName = updated.FirstName,
            LastName = updated.LastName,
            FullName = updated.FullName,
            Email = updated.Email?.Value,
            PhoneNumber = updated.PhoneNumber?.GetFormatted(),
            Address = updated.Address?.GetFullAddress(),
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt,
            IsActive = updated.IsActive,
            Status = updated.Status
        };
    }
}