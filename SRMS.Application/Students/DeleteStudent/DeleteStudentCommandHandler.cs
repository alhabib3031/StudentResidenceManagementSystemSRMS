using MediatR;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IRepositories<Student> _studentRepository;
    
    public DeleteStudentCommandHandler(IRepositories<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        return await _studentRepository.DeleteAsync(request.Id);
    }
}