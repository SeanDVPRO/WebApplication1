using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _lastEmailSent = new();
        private readonly ConcurrentDictionary<string, int> _emailCounts = new();
        private readonly TimeSpan _resetPeriod = TimeSpan.FromHours(1);

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            await SendPasswordResetEmailAsync(toEmail, resetLink, userName, TimeSpan.FromHours(1));
        }

        public async Task SendEmailVerificationAsync(string email, string verificationLink, string userName)
        {
            await SendEmailVerificationAsync(email, verificationLink, userName, TimeSpan.FromHours(24));
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName, TimeSpan expiration)
        {
            var subject = "Password Reset Request";
            var expirationText = GetExpirationText(expiration);

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #007bff; text-align: center;'>🔐 Password Reset Request</h2>
                        <p>Hello <strong>{HttpUtility.HtmlEncode(userName)}</strong>,</p>
                        <p>We received a request to reset your password. If you made this request, please click the button below to reset your password:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                Reset Password
                            </a>
                        </div>
                        
                        <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0;'>
                            <p style='margin: 0; color: #856404;'><strong>⚠️ Security Notice:</strong></p>
                            <ul style='margin: 10px 0 0 0; color: #856404;'>
                                <li>This link will expire in <strong>{expirationText}</strong> for security reasons</li>
                                <li>For your security, the reset link is only accessible via the button above</li>
                                <li>Never share this email or the reset link with anyone</li>
                                <li>This link was generated for: {HttpUtility.HtmlEncode(toEmail)}</li>
                            </ul>
                        </div>
                        
                        <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            This is an automated message from a secure system. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailWithRateLimitAsync(toEmail, subject, body, TimeSpan.FromMinutes(2), 5);
        }

        public async Task SendEmailVerificationAsync(string email, string verificationLink, string userName, TimeSpan expiration)
        {
            var subject = "Email Verification Required";
            var expirationText = GetExpirationText(expiration);

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9f9f9; padding: 20px;'>
                    <div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #4CAF50; margin: 0;'>Welcome to Our Platform!</h1>
                        </div>
                        
                        <h2 style='color: #333; margin-bottom: 20px;'>Hello {HttpUtility.HtmlEncode(userName)},</h2>
                        
                        <p style='color: #666; font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                            Thank you for registering with us! To complete your account setup and ensure the security of your account, 
                            please verify your email address by clicking the button below.
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationLink}' 
                               style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                      color: white; 
                                      padding: 15px 30px; 
                                      text-decoration: none; 
                                      border-radius: 25px; 
                                      font-weight: bold; 
                                      font-size: 16px;
                                      display: inline-block;
                                      box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);'>
                                ✓ Verify My Email Address
                            </a>
                        </div>
                        
                        <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0;'>
                            <p style='color: #856404; margin: 0; font-size: 14px;'>
                                <strong>Important:</strong> This verification link will expire in <strong>{expirationText}</strong> for security reasons.
                            </p>
                            <p style='color: #856404; margin: 10px 0 0 0; font-size: 12px;'>
                                For your protection, the verification link is only accessible via the secure button above.
                            </p>
                        </div>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                        
                        <p style='color: #999; font-size: 12px; line-height: 1.6; margin: 0;'>
                            If you did not create an account with us, please ignore this email. No further action is required.
                        </p>
                        
                        <div style='text-align: center; margin-top: 30px;'>
                            <p style='color: #666; font-size: 14px; margin: 0;'>
                                Best regards,<br>
                                <strong>Your Application Team</strong>
                            </p>
                        </div>
                    </div>
                </div>";

            await SendEmailWithRateLimitAsync(email, subject, htmlBody, TimeSpan.FromMinutes(5), 10);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();

                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@yourapp.com";
                var fromName = _configuration["EmailSettings:FromName"] ?? "Your App";
                message.From.Add(new MailboxAddress(Encoding.UTF8, fromName, fromEmail));

                message.To.Add(new MailboxAddress(Encoding.UTF8, "", toEmail));

                message.Subject = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(subject));

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = GeneratePlainTextFromHtml(body)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        private async Task SendEmailWithRateLimitAsync(string toEmail, string subject, string body,
            TimeSpan minIntervalBetweenEmails, int maxEmailsPerHour)
        {
            var now = DateTime.UtcNow;
            var cleanKey = toEmail.ToLowerInvariant();

            if (_lastEmailSent.TryGetValue(cleanKey, out var lastSent) &&
                now - lastSent < minIntervalBetweenEmails)
            {
                var waitTime = minIntervalBetweenEmails - (now - lastSent);
                throw new InvalidOperationException(
                    $"Please wait {waitTime.TotalMinutes:0} minute(s) before requesting another email.");
            }

            var hourKey = $"{cleanKey}_{now:yyyyMMddHH}";
            var count = _emailCounts.AddOrUpdate(hourKey, 1, (key, current) => current + 1);

            if (count > maxEmailsPerHour)
            {
                throw new InvalidOperationException(
                    $"Too many email requests. Please try again in {60 - now.Minute} minutes.");
            }

            _lastEmailSent[cleanKey] = now;
            await SendEmailAsync(toEmail, subject, body);
        }

        private string GetExpirationText(TimeSpan expiration)
        {
            if (expiration.TotalHours >= 1)
            {
                return $"{expiration.TotalHours:0} hour{(expiration.TotalHours > 1 ? "s" : "")}";
            }
            else
            {
                return $"{expiration.TotalMinutes:0} minute{(expiration.TotalMinutes > 1 ? "s" : "")}";
            }
        }

        private string GeneratePlainTextFromHtml(string html)
        {
            try
            {
                var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
                text = HttpUtility.HtmlDecode(text);
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

                text = text.Replace("Reset Password", "Reset Password: [Use the secure button in the email client]");
                text = text.Replace("Verify My Email Address", "Verify Email: [Use the secure button in the email client]");

                return text;
            }
            catch
            {
                return "Please view this email in an HTML-compatible email client to access the secure verification link.";
            }
        }

        public void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow - _resetPeriod;

            var oldKeys = _lastEmailSent.Where(x => x.Value < cutoff).Select(x => x.Key).ToList();
            foreach (var key in oldKeys)
            {
                _lastEmailSent.TryRemove(key, out _);
            }

            var oldHourKeys = _emailCounts.Keys.Where(k =>
            {
                var parts = k.Split('_');
                if (parts.Length == 2 && DateTime.TryParseExact(parts[1], "yyyyMMddHH", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    return date < DateTime.UtcNow.AddHours(-2);
                }
                return true;
            }).ToList();

            foreach (var key in oldHourKeys)
            {
                _emailCounts.TryRemove(key, out _);
            }
        }
    }
}