using Mapster;
using MediatR;
using SRMS.Application.Managers.DTOs;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Managers.GetManagerById;

public class GetManagerByIdQueryHandler : IRequestHandler<GetManagerByIdQuery, ManagerDto>
{
    private readonly IRepositories<Manager> _managerRepository;

    public GetManagerByIdQueryHandler(IRepositories<Manager> managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<ManagerDto> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
    {
        var managers = await _managerRepository.GetByIdAsync(request.Id);
        return managers.Adapt<ManagerDto>();
    }
}