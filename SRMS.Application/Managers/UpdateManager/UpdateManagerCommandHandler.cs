using Mapster;
using MediatR;
using SRMS.Application.Managers.DTOs;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Managers.UpdateManager;

public class UpdateManagerCommandHandler : IRequestHandler<UpdateManagerCommand, ManagerDto>
{
    private readonly IRepositories<Manager> _managerRepository;

    public UpdateManagerCommandHandler(IRepositories<Manager> managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<ManagerDto> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var existingManager = await _managerRepository.GetByIdAsync(request.Manager.Id);
        if (existingManager == null)
            return null!;
        
        // Update properties
        existingManager.FirstName = request.Manager.FirstName;
        existingManager.LastName = request.Manager.LastName;
        existingManager.Email = request.Manager.Email;
        existingManager.PhoneNumber = request.Manager.PhoneNumber;
        existingManager.Address = request.Manager.Address;
        existingManager.UpdatedAt = DateTime.UtcNow;

        var updatedManager = await _managerRepository.UpdateAsync(existingManager);
        return updatedManager.Adapt<ManagerDto>();

    }
}