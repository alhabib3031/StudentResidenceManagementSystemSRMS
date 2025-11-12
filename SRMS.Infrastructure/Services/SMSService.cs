using Microsoft.Extensions.Configuration;
using SRMS.Application.Notifications.Interfaces;

namespace SRMS.Infrastructure.Services;

public class SMSService : ISMSService
{
    private readonly IConfiguration _configuration;
    
    public SMSService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> SendSMSAsync(string phoneNumber, string message)
    {
        try
        {
            // TODO: Implement Twilio or other SMS provider
            // Example with Twilio:
            // var accountSid = _configuration["SMS:AccountSid"];
            // var authToken = _configuration["SMS:AuthToken"];
            // var fromNumber = _configuration["SMS:FromNumber"];
            // 
            // TwilioClient.Init(accountSid, authToken);
            // var messageResponse = await MessageResource.CreateAsync(
            //     to: new PhoneNumber(phoneNumber),
            //     from: new PhoneNumber(fromNumber),
            //     body: message
            // );
            // 
            // return messageResponse.Status == MessageResource.StatusEnum.Sent;
            
            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<bool> SendVerificationSMSAsync(string phoneNumber, string code)
    {
        var message = $"Your SRMS verification code is: {code}. Valid for 10 minutes.";
        return await SendSMSAsync(phoneNumber, message);
    }
    
    public async Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message)
    {
        var tasks = phoneNumbers.Select(phone => SendSMSAsync(phone, message));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }
}