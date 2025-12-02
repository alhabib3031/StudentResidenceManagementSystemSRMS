using Mapster;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Managers.DTOs;
using SRMS.Application.Students.CreateStudent;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Managers;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Managers.CreateManager;

public class CreateManagerCommandHandler : IRequestHandler<CreateManagerCommand, ManagerDto>
{
    private readonly IRepositories<Manager> _managerRepository;
    private readonly IAuditService _audit;
    
    public CreateManagerCommandHandler(IRepositories<Manager> managerRepository, IAuditService audit)
    {
        _managerRepository = managerRepository;
        _audit = audit;
    }

    public async Task<ManagerDto> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        // ✅ إنشاء Manager جديد
        var manager = new Manager
        {
            Id = Guid.NewGuid(),
            
            // Personal Information
            FirstName = request.Manager.FirstName,
            LastName = request.Manager.LastName,
            
            // Profile
            EmployeeNumber = request.Manager.EmployeeNumber,
            HireDate = request.Manager.HireDate,
            WorkingHoursStart = request.Manager.WorkingHoursStart,
            WorkingHoursEnd = request.Manager.WorkingHoursEnd,
            
            // Status
            Status = ManagerStatus.Active,
            
            // Audit
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
        
        // ✅ إنشاء Email (Value Object)
        try
        {
            manager.Email = !string.IsNullOrWhiteSpace(request.Manager.Email)
                ? Email.Create(request.Manager.Email)
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Manager",
                additionalInfo: $"Invalid email during manager creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid email: {ex.Message}");
        }
        
        // ✅ إنشاء PhoneNumber (Value Object)
        try
        {
            manager.PhoneNumber = !string.IsNullOrWhiteSpace(request.Manager.PhoneNumber)
                ? PhoneNumber.Create(
                    request.Manager.PhoneNumber,
                    request.Manager.PhoneCountryCode ?? "+218")
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Manager",
                additionalInfo: $"Invalid phone number during manager creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid phone number: {ex.Message}");
        }
        
        // ✅ إنشاء Address (Value Object)
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Manager.AddressCity))
            {
                manager.Address = Address.Create(
                    city: request.Manager.AddressCity,
                    street: request.Manager.AddressStreet ?? "",
                    state: request.Manager.AddressState ?? "",
                    postalCode: request.Manager.AddressPostalCode ?? "",
                    country: request.Manager.AddressCountry ?? "Libya"
                );
            }
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Manager",
                additionalInfo: $"Invalid address during manager creation: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid address: {ex.Message}");
        }
        
        // ✅ حفظ في قاعدة البيانات - تم تصحيح اسم المتغير
        var createdManager = await _managerRepository.CreateAsync(manager);
        
        // ✅ Log manager creation
        await _audit.LogCrudAsync(
            action: AuditAction.Create,
            newEntity: new
            {
                createdManager.Id,
                createdManager.FullName,
                createdManager.EmployeeNumber,
                Email = createdManager.Email?.Value,
                createdManager.Status,
                createdManager.HireDate
            },
            additionalInfo: $"New manager created: {createdManager.FullName} (Employee #: {createdManager.EmployeeNumber})"
        );
        
        // ✅ إرجاع DTO
        return new ManagerDto
        {
            Id = createdManager.Id,
            FirstName = createdManager.FirstName,
            LastName = createdManager.LastName,
            FullName = createdManager.FullName,
            Email = createdManager.Email?.Value,
            PhoneNumber = createdManager.PhoneNumber?.GetFormatted(),
            EmployeeNumber = createdManager.EmployeeNumber,
            Status = createdManager.Status,
            CreatedAt = createdManager.CreatedAt,
            UpdatedAt = createdManager.UpdatedAt,
            IsActive = createdManager.IsActive
        };
    }
}