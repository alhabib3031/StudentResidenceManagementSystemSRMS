using Mapster;
using MediatR;
using SRMS.Application.Managers.DTOs;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Managers.GetManager;

public class GetManagerQueryHandler : IRequestHandler<GetManagerQuery, IEnumerable<ManagerDto>>
{
    private readonly IRepositories<Manager> _managerRepository;
    
    public GetManagerQueryHandler(IRepositories<Manager> managerRepository)
    {
        _managerRepository = managerRepository;
    }
    
    public async Task<IEnumerable<ManagerDto>> Handle(GetManagerQuery request, CancellationToken cancellationToken)
    {
        var managers = await _managerRepository.GetAllAsync();

        return cancellationToken.IsCancellationRequested ? null! : managers.Adapt<IEnumerable<ManagerDto>>();
    }
}