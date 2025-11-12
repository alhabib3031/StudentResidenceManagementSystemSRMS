using SRMS.Application.Notifications.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class EmailService : IEmailService
{
    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendTemplateEmailAsync(string to, string templateName, object data)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendVerificationEmailAsync(string to, string verificationCode)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendPasswordResetEmailAsync(string to, string resetToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendWelcomeEmailAsync(string to, string userName)
    {
        throw new NotImplementedException();
    }
}