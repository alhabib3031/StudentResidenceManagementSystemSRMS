using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Managers.DeleteManager;

public class DeleteManagerCommandHandler : IRequestHandler<DeleteManagerCommand, bool>
{
    private readonly IRepositories<Manager> _managerRepository;
    private readonly IAuditService _audit;
    
    public DeleteManagerCommandHandler(IRepositories<Manager> managerRepository, IAuditService audit)
    {
        _managerRepository = managerRepository;
        _audit = audit;
    }

    public async Task<bool> Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await _managerRepository.GetByIdAsync(request.Id);
        
        if (manager == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Manager",
                request.Id.ToString(),
                additionalInfo: "Attempted to delete non-existent manager"
            );
            return false;
        }
        
        var managerInfo = new
        {
            manager.Id,
            manager.FullName,
            Email = manager.Email?.Value,
            manager.EmployeeNumber,
            manager.Status
        };
        
        var result = await _managerRepository.DeleteAsync(request.Id);
        
        if (result)
        {
            // ✅ Log manager deletion
            await _audit.LogCrudAsync(
                action: AuditAction.Delete,
                oldEntity: managerInfo,
                additionalInfo: $"Manager deleted (soft delete): {managerInfo.FullName} (Employee #: {managerInfo.EmployeeNumber})"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Manager",
                request.Id.ToString(),
                additionalInfo: $"Failed to delete manager: {managerInfo.FullName}"
            );
        }
        
        return result;
    }
}