using MediatR;

namespace SRMS.Application.Managers.DeleteManager;

public class DeleteManagerCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}