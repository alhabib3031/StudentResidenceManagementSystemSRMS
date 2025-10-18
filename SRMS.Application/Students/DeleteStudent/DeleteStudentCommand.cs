using MediatR;

namespace SRMS.Application.Students.DeleteStudent;

public class DeleteStudentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}