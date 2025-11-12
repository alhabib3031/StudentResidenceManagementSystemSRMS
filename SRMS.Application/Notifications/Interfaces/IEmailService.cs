namespace SRMS.Application.Notifications.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendTemplateEmailAsync(string to, string templateName, object data);
    Task<bool> SendVerificationEmailAsync(string to, string verificationCode);
    Task<bool> SendPasswordResetEmailAsync(string to, string resetToken);
    Task<bool> SendWelcomeEmailAsync(string to, string userName);
}