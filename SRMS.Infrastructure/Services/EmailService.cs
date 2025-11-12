using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SRMS.Application.Notifications.Interfaces;

namespace SRMS.Infrastructure.Services;

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
            
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };
            
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
    
    public async Task<bool> SendVerificationEmailAsync(string to, string verificationCode)
    {
        var subject = "Verify Your Email - SRMS";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Welcome to SRMS!</h2>
                    <p>Please verify your email by clicking the link below:</p>
                    <a href='https://yourdomain.com/verify-email?code={verificationCode}' 
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
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>You requested to reset your password. Click the link below:</p>
                    <a href='https://yourdomain.com/reset-password?token={resetToken}' 
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
