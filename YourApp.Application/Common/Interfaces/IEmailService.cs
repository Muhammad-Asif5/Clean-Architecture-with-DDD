using YourApp.Domain.Entities;

namespace YourApp.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(ApplicationUser user, string token);
        Task SendPasswordResetAsync(ApplicationUser user, string token);
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}