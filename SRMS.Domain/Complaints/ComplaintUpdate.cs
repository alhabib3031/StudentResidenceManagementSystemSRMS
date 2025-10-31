using SRMS.Domain.Complaints.Enums;

namespace SRMS.Domain.Complaints;

public class ComplaintUpdate
{
    public Guid Id { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public ComplaintStatus? Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private ComplaintUpdate() { }
    
    public static ComplaintUpdate Create(string message, ComplaintStatus? status = null)
    {
        return new ComplaintUpdate
        {
            Id = Guid.NewGuid(),
            Message = message,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}