using MediatR;
using SRMS.Application.Managers.DTOs;

namespace SRMS.Application.Managers.CreateManager;

public class CreateManagerCommand : IRequest<ManagerDto>
{
    public CreateManagerDto Manager { get; set; } = new();
}