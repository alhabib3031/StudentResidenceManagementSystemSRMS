using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Managers.DeleteManager;

public class DeleteManagerCommandHandler : IRequestHandler<DeleteManagerCommand, bool>
{
    private readonly IRepositories<Manager> _managerRepository;

    public DeleteManagerCommandHandler(IRepositories<Manager> managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<bool> Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await _managerRepository.GetByIdAsync(request.Id);

        if (manager is null) return false;

        var managerInfo = new
        {
            manager.Id,
            manager.FullName,
            Email = manager.Email?.Value,
            manager.EmployeeNumber,
            manager.Status
        };

        var result = await _managerRepository.DeleteAsync(request.Id);

        return result;
    }
}