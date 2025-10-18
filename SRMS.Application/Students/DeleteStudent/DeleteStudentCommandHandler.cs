using MediatR;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IStudentRepository _studentRepository;
    
    public DeleteStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        return await _studentRepository.DeleteAsync(request.Id);
    }
}