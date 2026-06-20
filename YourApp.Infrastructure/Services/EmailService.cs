using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Entities;

namespace YourApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IAppSettingService _appSettingService;

        public EmailService(
            ILogger<EmailService> logger,
            IAppSettingService appSettingService)
        {
            _logger = logger;
            _appSettingService = appSettingService;
        }

        public async Task SendEmailConfirmationAsync(ApplicationUser user, string token)
        {
            var confirmationLink = $"https://localhost:7071/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var subject = "Confirm Your Email Address";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to YourApp!</h2>
                    <p>Hello {user.GetFullName()},</p>
                    <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}'>Confirm Email</a></p>
                    <p>If you did not create an account, please ignore this email.</p>
                    <br/>
                    <p>Best regards,</p>
                    <p>YourApp Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(user.Email, subject, body, true);
        }

        public async Task SendPasswordResetAsync(ApplicationUser user, string token)
        {
            var resetLink = $"https://yourapp.com/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var subject = "Reset Your Password";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {user.GetFullName()},</p>
                    <p>We received a request to reset your password. Click the link below to reset it:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <br/>
                    <p>Best regards,</p>
                    <p>YourApp Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(user.Email, subject, body, true);
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            // TODO: Implement actual email sending (SendGrid, SMTP, etc.)
            _logger.LogInformation("Email sent to: {To}, Subject: {Subject}", to, subject);

            // In development, log the body for debugging
            _logger.LogDebug("Email Body: {Body}", body);

            await Task.CompletedTask;
        }
    }
}