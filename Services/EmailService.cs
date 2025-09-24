using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            var subject = "Password Reset Request";
            
            var maskedLink = CreateMaskedUrl(resetLink);
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #007bff; text-align: center;'>🔐 Password Reset Request</h2>
                        <p>Hello <strong>{userName}</strong>,</p>
                        <p>We received a request to reset your password. If you made this request, please click the button below to reset your password:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                Reset Password
                            </a>
                        </div>
                        
                        <p>If the button doesn't work, you can copy and paste this secure link into your browser:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; border-left: 4px solid #007bff;'>
                            <p style='margin: 0; font-family: monospace; font-size: 12px; color: #666;'>
                                Secure Reset Link: {maskedLink}
                            </p>
                            <p style='margin: 5px 0 0 0; font-size: 11px; color: #999;'>
                                <em>Note: This is a secure, masked link for your protection. Click the button above to access it safely.</em>
                            </p>
                        </div>
                        
                        <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0;'>
                            <p style='margin: 0; color: #856404;'><strong>⚠️ Security Notice:</strong></p>
                            <ul style='margin: 10px 0 0 0; color: #856404;'>
                                <li>This link will expire in <strong>1 hour</strong> for security reasons</li>
                                <li>Only use the button above or copy the full link from a trusted email client</li>
                                <li>Never share this link with anyone</li>
                            </ul>
                        </div>
                        
                        <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            This is an automated message from a secure system. Please do not reply to this email.<br>
                            For security reasons, sensitive information in this email has been masked.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailVerificationAsync(string email, string verificationLink, string userName)
        {
            var subject = "Email Verification Required";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9f9f9; padding: 20px;'>
                    <div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #4CAF50; margin: 0;'>Welcome to Our Platform!</h1>
                        </div>
                        
                        <h2 style='color: #333; margin-bottom: 20px;'>Hello {userName},</h2>
                        
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
                                <strong>Important:</strong> This verification link will expire in 24 hours for security reasons.
                            </p>
                        </div>
                        
                        <p style='color: #666; font-size: 14px; line-height: 1.6; margin-bottom: 10px;'>
                            If you're unable to click the button above, you can copy and paste the following link into your browser:
                        </p>
                        
                        <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all; font-size: 12px; color: #666;'>
                            {verificationLink}
                        </p>
                        
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

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@yourapp.com";
                var fromName = _configuration["EmailSettings:FromName"] ?? "Your App";
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                
                message.To.Add(new MailboxAddress("", toEmail));
                
                message.Subject = subject;
                
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
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
                
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                throw;
            }
        }

        private string CreateMaskedUrl(string originalUrl)
        {
            try
            {
                var uri = new Uri(originalUrl);
                var domain = uri.Host;
                var path = uri.AbsolutePath;
                
                var maskedPath = path.Length > 20 ? path.Substring(0, 15) + "..." : path;
                
                return $"https://{domain}{maskedPath}?token=***SECURE_TOKEN***";
            }
            catch
            {
                return "***SECURE_RESET_LINK*** (Use the button above)";
            }
        }
    }
}