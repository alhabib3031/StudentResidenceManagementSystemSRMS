using Mapster;
using MediatR;
using SRMS.Application.Students.DTOs;
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
        var existing = await _studentRepository.GetByIdAsync(request.Student.Id);
        
        if (existing == null)
            return null;
        
        // ✅ تحديث الخصائص البسيطة
        existing.FirstName = request.Student.FirstName;
        existing.LastName = request.Student.LastName;
        existing.NationalId = request.Student.NationalId;
        existing.DateOfBirth = request.Student.DateOfBirth;
        existing.Gender = request.Student.Gender;
        
        // ✅ تحديث المعلومات الأكاديمية
        existing.UniversityName = request.Student.UniversityName;
        existing.StudentNumber = request.Student.StudentNumber;
        existing.Major = request.Student.Major;
        existing.AcademicYear = request.Student.AcademicYear;
        
        // ✅ تحديث معلومات الطوارئ
        existing.EmergencyContactName = request.Student.EmergencyContactName;
        existing.EmergencyContactRelation = request.Student.EmergencyContactRelation;
        
        // ✅ تحديث الحالة
        existing.Status = request.Student.Status;
        existing.RoomId = request.Student.RoomId;
        existing.ManagerId = request.Student.ManagerId;
        
        // ✅ تحديث Email (Value Object)
        try
        {
            existing.Email = !string.IsNullOrWhiteSpace(request.Student.Email)
                ? Email.Create(request.Student.Email)
                : null;
        }
        catch (ArgumentException ex)
        {
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
            throw new InvalidOperationException($"Invalid address: {ex.Message}");
        }
        
        // ✅ تحديث Audit fields
        existing.UpdatedAt = DateTime.UtcNow;
        
        // ✅ حفظ التغييرات
        var updated = await _studentRepository.UpdateAsync(existing);
        
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