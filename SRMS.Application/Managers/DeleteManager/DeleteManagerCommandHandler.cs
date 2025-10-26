using MediatR;
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

    public Task<bool> Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        return _managerRepository.DeleteAsync(request.Id);
    }
}