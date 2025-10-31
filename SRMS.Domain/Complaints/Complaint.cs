using SRMS.Domain.Abstractions;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Students;

namespace SRMS.Domain.Complaints;

/// <summary>
/// Complaint (الشكوى)
/// </summary>
public class Complaint : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ComplaintNumber { get; private set; } = string.Empty;
    
    // Student
    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    
    // Category & Priority
    public ComplaintCategory Category { get; private set; }
    public ComplaintPriority Priority { get; private set; }
    public ComplaintStatus Status { get; private set; }
    
    // Assignment
    public Guid? AssignedTo { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    
    // Resolution
    public string? Resolution { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    
    // Attachments
    private readonly List<string> _attachments = new();
    public IReadOnlyCollection<string> Attachments => _attachments.AsReadOnly();
    
    // Tracking
    private readonly List<ComplaintUpdate> _updates = new();
    public IReadOnlyCollection<ComplaintUpdate> Updates => _updates.AsReadOnly();
    
    private Complaint() { }
    
    public static Complaint Create(
        Guid studentId,
        string title,
        string description,
        ComplaintCategory category,
        ComplaintPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Complaint title is required");
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Complaint description is required");
        
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            ComplaintNumber = GenerateComplaintNumber(),
            StudentId = studentId,
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            Status = ComplaintStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };
        
        complaint.AddUpdate("Complaint submitted", ComplaintStatus.Submitted);
        
        return complaint;
    }
    
    private static string GenerateComplaintNumber()
    {
        return $"COMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
    }
    
    public void AssignTo(Guid assigneeId)
    {
        if (Status == ComplaintStatus.Resolved || Status == ComplaintStatus.Closed)
            throw new InvalidOperationException("Cannot assign resolved or closed complaint");
        
        AssignedTo = assigneeId;
        AssignedAt = DateTime.UtcNow;
        Status = ComplaintStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
        
        AddUpdate($"Complaint assigned", ComplaintStatus.InProgress);
    }
    
    public void AddUpdate(string message, ComplaintStatus? newStatus = null)
    {
        var update = ComplaintUpdate.Create(message, newStatus);
        _updates.Add(update);
        
        if (newStatus.HasValue)
            Status = newStatus.Value;
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Resolve(string resolution, Guid resolvedById)
    {
        if (Status == ComplaintStatus.Resolved || Status == ComplaintStatus.Closed)
            throw new InvalidOperationException("Complaint already resolved");
        
        Resolution = resolution;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedById;
        Status = ComplaintStatus.Resolved;
        UpdatedAt = DateTime.UtcNow;
        
        AddUpdate($"Complaint resolved: {resolution}", ComplaintStatus.Resolved);
    }
    
    public void Close()
    {
        if (Status != ComplaintStatus.Resolved)
            throw new InvalidOperationException("Can only close resolved complaints");
        
        Status = ComplaintStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        
        AddUpdate("Complaint closed", ComplaintStatus.Closed);
    }
    
    public void Reopen(string reason)
    {
        if (Status != ComplaintStatus.Closed)
            throw new InvalidOperationException("Can only reopen closed complaints");
        
        Status = ComplaintStatus.InProgress;
        Resolution = null;
        ResolvedAt = null;
        ResolvedBy = null;
        UpdatedAt = DateTime.UtcNow;
        
        AddUpdate($"Complaint reopened: {reason}", ComplaintStatus.InProgress);
    }
    
    public void AddAttachment(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty");
        
        _attachments.Add(filePath);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ChangePriority(ComplaintPriority newPriority)
    {
        if (Priority == newPriority)
            return;
        
        var oldPriority = Priority;
        Priority = newPriority;
        UpdatedAt = DateTime.UtcNow;
        
        AddUpdate($"Priority changed from {oldPriority} to {newPriority}");
    }
}
