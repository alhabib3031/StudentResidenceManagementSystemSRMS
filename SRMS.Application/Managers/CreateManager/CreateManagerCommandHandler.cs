using Mapster;
using MediatR;
using SRMS.Application.Managers.DTOs;
using SRMS.Application.Students.CreateStudent;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Managers.CreateManager;

public class CreateManagerCommandHandler : IRequestHandler<CreateManagerCommand, ManagerDto>
{
    private readonly IRepositories<Manager> _managerRepository;
    
    public CreateManagerCommandHandler(IRepositories<Manager> managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<ManagerDto> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        // Map من DTO إلى Entity
        var manager = request.Manager.Adapt<Manager>();
        manager.Id = Guid.NewGuid();
        manager.CreatedAt = DateTime.UtcNow;
        manager.UpdatedAt = DateTime.UtcNow;
        manager.IsActive = true;
        manager.IsDeleted = false;
        
        var createdStudent = await _managerRepository.CreateAsync(manager);
        
        // Map من Entity إلى DTO للإرجاع
        return createdStudent.Adapt<ManagerDto>();
    }
}