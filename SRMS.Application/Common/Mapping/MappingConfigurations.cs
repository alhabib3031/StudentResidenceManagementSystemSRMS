using Mapster;
using SRMS.Application.Managers.DTOs;
using SRMS.Application.Students.DTOs.StudentDTOs;
using SRMS.Domain.Managers;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Common.Mapping;

/// <summary>
/// تكوينات التحويل العامة لجميع أنواع الكيانات
/// </summary>
public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // تكوينات تحويل عامة لجميع الكيانات
        RegisterValueObjectMappings(config);
        
        // تكوينات خاصة بالطلاب
        RegisterStudentMappings(config);
        
        // تكوينات خاصة بالمديرين
        RegisterManagerMappings(config);
    }

    /// <summary>
    /// تسجيل تكوينات تحويل Value Objects
    /// </summary>
    private void RegisterValueObjectMappings(TypeAdapterConfig config)
    {
        // تكوين تحويل Email
        config.ForType<string, Email>()
            .MapWith(src => Email.Create(src));
            
        config.ForType<Email, string>()
            .MapWith(src => src == null ? null : src.Value);
            
        // تكوين تحويل PhoneNumber
        config.ForType<string, PhoneNumber>()
            .MapWith(src => string.IsNullOrEmpty(src) ? null : PhoneNumber.Create(src.Value));
            
        config.ForType<PhoneNumber, string>()
            .MapWith(src => src == null ? null : src.Value);
            
        // تكوين تحويل Address
        config.ForType<string, Address>()
            .MapWith(src => string.IsNullOrEmpty(src) ? null : Address.Create(src));
            
        config.ForType<Address, string>()
            .MapWith(src => src == null ? null : src.);
    }
    
    /// <summary>
    /// تسجيل تكوينات تحويل خاصة بالطلاب
    /// </summary>
    private void RegisterStudentMappings(TypeAdapterConfig config)
    {
        // CreateStudentDto -> Student
        config.NewConfig<CreateStudentDto, Student>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.Status);
            
        // Student -> StudentDto
        config.NewConfig<Student, StudentDto>();
        
        // إضافة المزيد من تكوينات الطلاب حسب الحاجة
        config.NewConfig<UpdateStudentDto, Student>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted);
    }
    
    /// <summary>
    /// تسجيل تكوينات تحويل خاصة بالمديرين
    /// </summary>
    private void RegisterManagerMappings(TypeAdapterConfig config)
    {
        // CreateManagerDto -> Manager
        config.NewConfig<CreateManagerDto, Manager>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.IsDeleted);
            
        // Manager -> ManagerDto
        config.NewConfig<Manager, ManagerDto>();
        
        // إضافة المزيد من تكوينات المديرين حسب الحاجة
        config.NewConfig<UpdateManagerDto, Manager>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted);
    }
}