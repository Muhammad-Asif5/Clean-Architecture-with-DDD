using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Entities;
using YourApp.Domain.Enums;
using YourApp.Domain.Settings;
using YourApp.Infrastructure.Persistence.Context;

namespace YourApp.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationDbContext context,
            ILogger<IdentityService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<(bool Succeeded, string[] Errors)> CreateUserAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created: {Email}", user.Email);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User updated: {Email}", user.Email);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found" });

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User deleted: {Email}", user.Email);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task AddToRoleAsync(ApplicationUser user, List<string> roles)
        {
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var newRole = new IdentityRole<Guid> { Name = role };
                    await _roleManager.CreateAsync(newRole);
                }

                var result = await _userManager.AddToRoleAsync(user, role);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} added to role {Role}", user.Email, role);
                }

            }
        }

        public async Task<(bool Succeeded, string[] Errors)> RemoveFromRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} removed from role {Role}", user.Email, role);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<(bool Succeeded, string Token)> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return (true, token);
        }

        public async Task<(bool Succeeded, string[] Errors)> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                user.ConfirmEmail();
                await _userManager.UpdateAsync(user);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string Token)> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return (true, token);
        }

        public async Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                user.RecordPasswordChange();
                await _userManager.UpdateAsync(user);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                user.RecordPasswordChange();
                await _userManager.UpdateAsync(user);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> LockoutUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found" });

            // ✅ Use Identity's lockout
            var result = await _userManager.SetLockoutEnabledAsync(user, true);

            if (result.Succeeded)
            {
                // Set lockout end date
                var lockoutSettings = new LockoutSettings { UserLockoutTimeInMinutes = 15 };
                var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(lockoutSettings.UserLockoutTimeInMinutes);
                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

                user.UpdateStatus(UserStatus.Locked);
                await _userManager.UpdateAsync(user);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> UnlockUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found" });

            // ✅ Use Identity's unlock
            var result = await _userManager.SetLockoutEnabledAsync(user, false);

            if (result.Succeeded)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                user.ResetFailedLoginAttempts();
                await _userManager.UpdateAsync(user);
                return (true, Array.Empty<string>());
            }

            return (false, result.Errors.Select(e => e.Description).ToArray());
        }
    }
}