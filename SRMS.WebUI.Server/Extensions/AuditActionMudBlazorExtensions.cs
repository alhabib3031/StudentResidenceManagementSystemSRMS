// ════════════════════════════════════════════════════════════
// SRMS.WebUI.Server/Extensions/AuditActionMudBlazorExtensions.cs
// ════════════════════════════════════════════════════════════
using MudBlazor;
using SRMS.Application.AuditLogs.Extensions;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.WebUI.Server.Extensions;

/// <summary>
/// Extension methods for AuditAction to work with MudBlazor components
/// </summary>
public static class AuditActionMudBlazorExtensions
{
    /// <summary>
    /// Get MudBlazor Color for AuditAction
    /// </summary>
    public static Color GetColor(this AuditAction action) => action switch
    {
        // CRUD Operations
        AuditAction.Create => Color.Success,
        AuditAction.Read => Color.Info,
        AuditAction.Update => Color.Warning,
        AuditAction.Delete => Color.Error,
        AuditAction.Restore => Color.Success,
        
        // Authentication
        AuditAction.Login => Color.Primary,
        AuditAction.Logout => Color.Secondary,
        AuditAction.LoginFailed => Color.Error,
        AuditAction.PasswordChanged => Color.Success,
        AuditAction.PasswordReset => Color.Warning,
        AuditAction.EmailVerified => Color.Success,
        AuditAction.TwoFactorEnabled => Color.Success,
        AuditAction.TwoFactorDisabled => Color.Warning,
        AuditAction.AccountLocked => Color.Error,
        AuditAction.AccountUnlocked => Color.Success,
        
        // Roles & Permissions
        AuditAction.RoleAssigned => Color.Primary,
        AuditAction.RoleRemoved => Color.Warning,
        AuditAction.PermissionGranted => Color.Success,
        AuditAction.PermissionRevoked => Color.Warning,
        
        // Data Operations
        AuditAction.Export => Color.Info,
        AuditAction.Import => Color.Info,
        AuditAction.Download => Color.Info,
        AuditAction.Upload => Color.Info,
        AuditAction.Print => Color.Info,
        
        // Student Operations
        AuditAction.StudentRegistered => Color.Success,
        AuditAction.StudentActivated => Color.Success,
        AuditAction.StudentSuspended => Color.Warning,
        AuditAction.StudentGraduated => Color.Info,
        AuditAction.StudentWithdrawn => Color.Secondary,
        AuditAction.RoomAssigned => Color.Primary,
        AuditAction.RoomUnassigned => Color.Warning,
        
        // Payment Operations
        AuditAction.PaymentCreated => Color.Info,
        AuditAction.PaymentPaid => Color.Success,
        AuditAction.PaymentCancelled => Color.Error,
        AuditAction.PaymentRefunded => Color.Warning,
        AuditAction.PaymentOverdue => Color.Error,
        
        // Complaint Operations
        AuditAction.ComplaintSubmitted => Color.Info,
        AuditAction.ComplaintAssigned => Color.Primary,
        AuditAction.ComplaintResolved => Color.Success,
        AuditAction.ComplaintClosed => Color.Secondary,
        AuditAction.ComplaintReopened => Color.Warning,
        
        // System Operations
        AuditAction.SystemStarted => Color.Success,
        AuditAction.SystemShutdown => Color.Error,
        AuditAction.ConfigurationChanged => Color.Warning,
        AuditAction.BackupCreated => Color.Success,
        AuditAction.BackupRestored => Color.Warning,
        AuditAction.DatabaseMigration => Color.Info,
        
        // Notifications
        AuditAction.NotificationSent => Color.Info,
        AuditAction.EmailSent => Color.Info,
        AuditAction.SMSSent => Color.Info,
        
        // Security
        AuditAction.UnauthorizedAccess => Color.Error,
        AuditAction.SuspiciousActivity => Color.Warning,
        AuditAction.SecurityAlertTriggered => Color.Error,
        
        // Errors
        AuditAction.Error => Color.Error,
        AuditAction.Failure => Color.Error,
        
        _ => Color.Default
    };
    
    /// <summary>
    /// Get MudBlazor Icon for AuditAction
    /// </summary>
    public static string GetIcon(this AuditAction action) => action switch
    {
        // CRUD Operations
        AuditAction.Create => Icons.Material.Filled.Add,
        AuditAction.Read => Icons.Material.Filled.Visibility,
        AuditAction.Update => Icons.Material.Filled.Edit,
        AuditAction.Delete => Icons.Material.Filled.Delete,
        AuditAction.Restore => Icons.Material.Filled.Restore,
        
        // Authentication
        AuditAction.Login => Icons.Material.Filled.Login,
        AuditAction.Logout => Icons.Material.Filled.Logout,
        AuditAction.LoginFailed => Icons.Material.Filled.ErrorOutline,
        AuditAction.PasswordChanged => Icons.Material.Filled.Key,
        AuditAction.PasswordReset => Icons.Material.Filled.LockReset,
        AuditAction.EmailVerified => Icons.Material.Filled.MarkEmailRead,
        AuditAction.TwoFactorEnabled => Icons.Material.Filled.Security,
        AuditAction.TwoFactorDisabled => Icons.Material.Filled.SecurityUpdateWarning,
        AuditAction.AccountLocked => Icons.Material.Filled.Lock,
        AuditAction.AccountUnlocked => Icons.Material.Filled.LockOpen,
        
        // Roles & Permissions
        AuditAction.RoleAssigned => Icons.Material.Filled.PersonAdd,
        AuditAction.RoleRemoved => Icons.Material.Filled.PersonRemove,
        AuditAction.PermissionGranted => Icons.Material.Filled.CheckCircle,
        AuditAction.PermissionRevoked => Icons.Material.Filled.Cancel,
        
        // Data Operations
        AuditAction.Export => Icons.Material.Filled.Download,
        AuditAction.Import => Icons.Material.Filled.Upload,
        AuditAction.Download => Icons.Material.Filled.GetApp,
        AuditAction.Upload => Icons.Material.Filled.CloudUpload,
        AuditAction.Print => Icons.Material.Filled.Print,
        
        // Student Operations
        AuditAction.StudentRegistered => Icons.Material.Filled.PersonAdd,
        AuditAction.StudentActivated => Icons.Material.Filled.CheckCircle,
        AuditAction.StudentSuspended => Icons.Material.Filled.Block,
        AuditAction.StudentGraduated => Icons.Material.Filled.School,
        AuditAction.StudentWithdrawn => Icons.Material.Filled.ExitToApp,
        AuditAction.RoomAssigned => Icons.Material.Filled.MeetingRoom,
        AuditAction.RoomUnassigned => Icons.Material.Filled.NoMeetingRoom,
        
        // Payment Operations
        AuditAction.PaymentCreated => Icons.Material.Filled.Receipt,
        AuditAction.PaymentPaid => Icons.Material.Filled.Paid,
        AuditAction.PaymentCancelled => Icons.Material.Filled.Cancel,
        AuditAction.PaymentRefunded => Icons.Material.Filled.MoneyOff,
        AuditAction.PaymentOverdue => Icons.Material.Filled.Warning,
        
        // Complaint Operations
        AuditAction.ComplaintSubmitted => Icons.Material.Filled.ReportProblem,
        AuditAction.ComplaintAssigned => Icons.Material.Filled.Assignment,
        AuditAction.ComplaintResolved => Icons.Material.Filled.CheckCircle,
        AuditAction.ComplaintClosed => Icons.Material.Filled.Close,
        AuditAction.ComplaintReopened => Icons.Material.Filled.Replay,
        
        // System Operations
        AuditAction.SystemStarted => Icons.Material.Filled.PowerSettingsNew,
        AuditAction.SystemShutdown => Icons.Material.Filled.PowerOff,
        AuditAction.ConfigurationChanged => Icons.Material.Filled.Settings,
        AuditAction.BackupCreated => Icons.Material.Filled.Backup,
        AuditAction.BackupRestored => Icons.Material.Filled.SettingsBackupRestore,
        AuditAction.DatabaseMigration => Icons.Material.Filled.Storage,
        
        // Notifications
        AuditAction.NotificationSent => Icons.Material.Filled.Notifications,
        AuditAction.EmailSent => Icons.Material.Filled.Email,
        AuditAction.SMSSent => Icons.Material.Filled.Sms,
        
        // Security
        AuditAction.UnauthorizedAccess => Icons.Material.Filled.Block,
        AuditAction.SuspiciousActivity => Icons.Material.Filled.Warning,
        AuditAction.SecurityAlertTriggered => Icons.Material.Filled.GppBad,
        
        // Errors
        AuditAction.Error => Icons.Material.Filled.Error,
        AuditAction.Failure => Icons.Material.Filled.ErrorOutline,
        
        _ => Icons.Material.Filled.Info
    };
    
    /// <summary>
    /// Get CSS class for badge styling
    /// </summary>
    public static string GetBadgeClass(this AuditAction action) => action switch
    {
        AuditAction.Create or AuditAction.Restore => "badge-success",
        AuditAction.Update or AuditAction.Download => "badge-warning",
        AuditAction.Delete or AuditAction.Error or AuditAction.Failure => "badge-danger",
        AuditAction.Read or AuditAction.Import => "badge-info",
        AuditAction.Login or AuditAction.Logout => "badge-primary",
        _ => "badge-secondary"
    };
    
    /// <summary>
    /// Get severity level for notifications/alerts
    /// </summary>
    public static Severity GetSeverity(this AuditAction action) => action switch
    {
        AuditAction.Create or 
        AuditAction.StudentRegistered or 
        AuditAction.PaymentPaid or 
        AuditAction.ComplaintResolved => Severity.Success,
        
        AuditAction.Update or 
        AuditAction.PasswordChanged or 
        AuditAction.ConfigurationChanged => Severity.Warning,
        
        AuditAction.Delete or 
        AuditAction.Error or 
        AuditAction.Failure or 
        AuditAction.LoginFailed or 
        AuditAction.UnauthorizedAccess or 
        AuditAction.AccountLocked => Severity.Error,
        
        AuditAction.Read or 
        AuditAction.Export or 
        AuditAction.Import => Severity.Info,
        
        _ => Severity.Normal
    };
}