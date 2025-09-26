using System;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
        Task SendEmailVerificationAsync(string email, string verificationLink, string userName);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}