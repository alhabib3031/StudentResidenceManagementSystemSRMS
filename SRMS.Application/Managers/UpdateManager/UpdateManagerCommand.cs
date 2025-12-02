using MediatR;
using SRMS.Application.Managers.DTOs;

namespace SRMS.Application.Managers.UpdateManager;

public class UpdateManagerCommand : IRequest<ManagerDto>
{
    public UpdateManagerDto Manager { get; set; } = new();
}