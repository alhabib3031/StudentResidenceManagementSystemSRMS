using MediatR;
using SRMS.Application.Managers.DTOs;

namespace SRMS.Application.Managers.GetManager;

public class GetManagerQuery : IRequest<IEnumerable<ManagerDto>>
{
    
}