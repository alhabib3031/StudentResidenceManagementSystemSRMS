using Mapster;
using MediatR;
using SRMS.Application.Managers.DTOs;
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
        var existingManager = await _managerRepository.GetByIdAsync(request.Manager.Id);
        if (existingManager == null)
            return null!;
        
        // Update properties
        existingManager.FirstName = request.Manager.FirstName;
        existingManager.LastName = request.Manager.LastName;
        
        // Properly convert string values to value objects
        existingManager.Email = !string.IsNullOrEmpty(request.Manager.Email) 
            ? Email.Create(request.Manager.Email)
            : null;
            
        existingManager.PhoneNumber = !string.IsNullOrEmpty(request.Manager.PhoneNumber)
            ? PhoneNumber.Create(request.Manager.PhoneNumber)
            : null;

        if (existingManager.Address != null)
            existingManager.Address = !string.IsNullOrEmpty(request.Manager.Address)
                ? Address.Create(
                    existingManager.Address.Street,
                    existingManager.Address.City,
                    existingManager.Address.State,
                    existingManager.Address.PostalCode,
                    existingManager.Address.Country
                )
                : null;

        existingManager.UpdatedAt = DateTime.UtcNow;

        var updatedManager = await _managerRepository.UpdateAsync(existingManager);
        return updatedManager.Adapt<ManagerDto>();

    }
}