using SRMS.Application.Notifications.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class SMSService : ISMSService
{
    public Task<bool> SendSMSAsync(string phoneNumber, string message)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendVerificationSMSAsync(string phoneNumber, string code)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message)
    {
        throw new NotImplementedException();
    }
}