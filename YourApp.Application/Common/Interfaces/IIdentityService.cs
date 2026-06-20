using YourApp.Domain.Entities;

namespace YourApp.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        // User Management
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetUserByUsernameAsync(string username);

        Task<(bool Succeeded, string[] Errors)> CreateUserAsync(
            ApplicationUser user,
            string password);

        Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(ApplicationUser user);
        Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId);

        // Role Management
        Task AddToRoleAsync(ApplicationUser user, List<string> roles);

        Task<(bool Succeeded, string[] Errors)> RemoveFromRoleAsync(
            ApplicationUser user,
            string role);

        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);

        // Authentication
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

        // Email Confirmation
        Task<(bool Succeeded, string Token)> GenerateEmailConfirmationTokenAsync(
            ApplicationUser user);

        Task<(bool Succeeded, string[] Errors)> ConfirmEmailAsync(
            ApplicationUser user,
            string token);

        // Password Management
        Task<(bool Succeeded, string Token)> GeneratePasswordResetTokenAsync(
            ApplicationUser user);

        Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(
            ApplicationUser user,
            string token,
            string newPassword);

        Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword);

        // Account Lockout
        Task<(bool Succeeded, string[] Errors)> LockoutUserAsync(string userId);
        Task<(bool Succeeded, string[] Errors)> UnlockUserAsync(string userId);
    }
}