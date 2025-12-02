using MediatR;
using SRMS.Application.Managers.DTOs;

namespace SRMS.Application.Managers.GetManagerById;

public class GetManagerByIdQuery : IRequest<ManagerDto>
{
    public Guid Id { get; set; }
}