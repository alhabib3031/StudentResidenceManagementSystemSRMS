using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Students.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IRepositories<Student> _studentRepository;
    private readonly IAuditService _audit;
    
    public DeleteStudentCommandHandler(IRepositories<Student> studentRepository, IAuditService audit)
    {
        _studentRepository = studentRepository;
        _audit = audit;
    }
    
    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        
        if (student == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Student",
                request.Id.ToString(),
                additionalInfo: "Attempted to delete non-existent student"
            );
            return false;
        }
        
        var studentInfo = new
        {
            student.Id,
            student.FullName,
            Email = student.Email?.Value,
            student.StudentNumber,
            student.UniversityName,
            student.Status
        };
        
        var result = await _studentRepository.DeleteAsync(request.Id);
        
        if (result)
        {
            // ✅ Log student deletion
            await _audit.LogCrudAsync(
                action: AuditAction.Delete,
                oldEntity: studentInfo,
                additionalInfo: $"Student deleted (soft delete): {studentInfo.FullName} (Student #: {studentInfo.StudentNumber})"
            );
        }
        else
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Student",
                request.Id.ToString(),
                additionalInfo: $"Failed to delete student: {studentInfo.FullName}"
            );
        }
        
        return result;
    }
}