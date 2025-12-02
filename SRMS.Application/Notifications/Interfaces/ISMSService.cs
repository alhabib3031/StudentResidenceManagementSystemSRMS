namespace SRMS.Application.Notifications.Interfaces;

public interface ISMSService
{
    Task<bool> SendSMSAsync(string phoneNumber, string message);
    Task<bool> SendVerificationSMSAsync(string phoneNumber, string code);
    Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message);
}