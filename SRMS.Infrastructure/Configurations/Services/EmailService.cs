using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using SRMS.Application.Notifications.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "SRMS";
            
            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            
            message.To.Add(to);
            
            await client.SendMailAsync(message);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task<bool> SendTemplateEmailAsync(string to, string templateName, object data)
    {
        // TODO: Implement email template rendering (using Razor, etc.)
        return await Task.FromResult(true);
    }
    
    public async Task<bool> SendVerificationEmailAsync(string to, string userId, string encodedVerificationCode)
    {
        var subject = "Verify Your Email - SRMS";
        // Get the base URL from configuration or use a default
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7117";
        // URL-encode the token to ensure it's safe in the URL
        // var encodedToken = System.Net.WebUtility.UrlEncode(verificationCode);

        var verificationLink = $"{baseUrl}/verify-email?userId={userId}&code={encodedVerificationCode}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Welcome to SRMS!</h2>
                    <p>Please verify your email by clicking the link below:</p>
                    <a href='{verificationLink}' 
                       style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Verify Email
                    </a>
                    <p style='margin-top: 20px; color: #666;'>If you didn't create an account, please ignore this email.</p>
                </div>
            </body>
            </html>
        ";
        
        return await SendEmailAsync(to, subject, body, isHtml: true);
    }
    
    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetToken)
    {
        var subject = "Reset Your Password - SRMS";
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7117";
        var encodedToken = System.Net.WebUtility.UrlEncode(resetToken);
        var encodedEmail = System.Net.WebUtility.UrlEncode(to);
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>You requested to reset your password. Click the link below:</p>
                    <a href='{baseUrl}/reset-password?email={encodedEmail}&token={encodedToken}' 
                       style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Reset Password
                    </a>
                    <p style='margin-top: 20px; color: #666;'>If you didn't request this, please ignore this email.</p>
                    <p style='color: #999; font-size: 12px;'>This link expires in 24 hours.</p>
                </div>
            </body>
            </html>
        ";
        
        return await SendEmailAsync(to, subject, body, isHtml: true);
    }
    
    public async Task<bool> SendWelcomeEmailAsync(string to, string userName)
    {
        var subject = "Welcome to SRMS!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Welcome, {userName}!</h2>
                    <p>Your email has been verified successfully.</p>
                    <p>You can now log in and start using the Student Residence Management System.</p>
                    <a href='https://yourdomain.com/login' 
                       style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Login Now
                    </a>
                    <p style='margin-top: 20px; color: #666;'>If you have any questions, feel free to contact our support team.</p>
                </div>
            </body>
            </html>
        ";
        
        return await SendEmailAsync(to, subject, body, isHtml: true);
    }
}
