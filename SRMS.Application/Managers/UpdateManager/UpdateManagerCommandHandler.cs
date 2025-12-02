using Mapster;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Managers.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Managers.UpdateManager;

public class UpdateManagerCommandHandler : IRequestHandler<UpdateManagerCommand, ManagerDto?>
{
    private readonly IRepositories<Manager> _managerRepository;
    private readonly IAuditService _audit;

    public UpdateManagerCommandHandler(IRepositories<Manager> managerRepository, IAuditService audit)
    {
        _managerRepository = managerRepository;
        _audit = audit;
    }

    public async Task<ManagerDto?> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _managerRepository.GetByIdAsync(request.Manager.Id);
        
        if (existing == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Manager",
                request.Manager.Id.ToString(),
                additionalInfo: "Attempted to update non-existent manager"
            );
            return null;
        }
        
        // Store old values
        var oldValues = new
        {
            existing.FirstName,
            existing.LastName,
            Email = existing.Email?.Value,
            PhoneNumber = existing.PhoneNumber?.GetFormatted(),
            existing.EmployeeNumber,
            existing.HireDate,
            existing.WorkingHoursStart,
            existing.WorkingHoursEnd,
            existing.Status
        };
        
        // ✅ تحديث الخصائص البسيطة
        existing.FirstName = request.Manager.FirstName;
        existing.LastName = request.Manager.LastName;
        existing.EmployeeNumber = request.Manager.EmployeeNumber;
        existing.HireDate = request.Manager.HireDate;
        existing.WorkingHoursStart = request.Manager.WorkingHoursStart;
        existing.WorkingHoursEnd = request.Manager.WorkingHoursEnd;
        existing.Status = request.Manager.Status;
        
        // ✅ تحديث Email (Value Object)
        try
        {
            existing.Email = !string.IsNullOrWhiteSpace(request.Manager.Email)
                ? Email.Create(request.Manager.Email)
                : null;
        }
        catch (ArgumentException ex)
        {
            await _audit.LogAsync(
                AuditAction.Error,
                "Manager",
                existing.Id.ToString(),
                additionalInfo: $"Invalid email during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid email: {ex.Message}");
        }
        
        // ✅ تحديث PhoneNumber (Value Object)
        try
        {
            existing.PhoneNumber = !string.IsNullOrWhiteSpace(request.Manager.PhoneNumber)
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
                existing.Id.ToString(),
                additionalInfo: $"Invalid phone number during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid phone number: {ex.Message}");
        }
        
        // ✅ تحديث Address (Value Object) - بالقيم الجديدة من DTO
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Manager.AddressCity))
            {
                existing.Address = Address.Create(
                    city: request.Manager.AddressCity,
                    street: request.Manager.AddressStreet ?? "",
                    state: request.Manager.AddressState ?? "",
                    postalCode: request.Manager.AddressPostalCode ?? "",
                    country: request.Manager.AddressCountry ?? "Libya"
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
                "Manager",
                existing.Id.ToString(),
                additionalInfo: $"Invalid address during update: {ex.Message}"
            );
            throw new InvalidOperationException($"Invalid address: {ex.Message}");
        }
        
        // ✅ تحديث Audit fields
        existing.UpdatedAt = DateTime.UtcNow;
        
        // ✅ حفظ التغييرات
        var updated = await _managerRepository.UpdateAsync(existing);
        
        // Store new values
        var newValues = new
        {
            updated.FirstName,
            updated.LastName,
            Email = updated.Email?.Value,
            PhoneNumber = updated.PhoneNumber?.GetFormatted(),
            updated.EmployeeNumber,
            updated.HireDate,
            updated.WorkingHoursStart,
            updated.WorkingHoursEnd,
            updated.Status
        };
        
        // ✅ Log manager update
        await _audit.LogCrudAsync(
            action: AuditAction.Update,
            oldEntity: oldValues,
            newEntity: newValues,
            additionalInfo: $"Manager updated: {updated.FullName}"
        );
        
        // ✅ Log status change if changed
        if (oldValues.Status != newValues.Status)
        {
            await _audit.LogAsync(
                action: AuditAction.Update,
                entityName: "Manager",
                entityId: updated.Id.ToString(),
                oldValues: new { Status = oldValues.Status },
                newValues: new { Status = newValues.Status },
                additionalInfo: $"Manager status changed from {oldValues.Status} to {newValues.Status}"
            );
        }
        
        // ✅ إرجاع DTO
        return new ManagerDto
        {
            Id = updated.Id,
            FirstName = updated.FirstName,
            LastName = updated.LastName,
            FullName = updated.FullName,
            Email = updated.Email?.Value,
            PhoneNumber = updated.PhoneNumber?.GetFormatted(),
            EmployeeNumber = updated.EmployeeNumber,
            Status = updated.Status,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt,
            IsActive = updated.IsActive
        };
    }
}