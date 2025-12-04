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

    public UpdateManagerCommandHandler(IRepositories<Manager> managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<ManagerDto?> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _managerRepository.GetByIdAsync(request.Manager.Id);

        if (existing is null) return null;

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
            throw new InvalidOperationException($"Invalid address: {ex.Message}");
        }

        // ✅ تحديث Audit fields
        existing.UpdatedAt = DateTime.UtcNow;

        // ✅ حفظ التغييرات
        var updated = await _managerRepository.UpdateAsync(existing);

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