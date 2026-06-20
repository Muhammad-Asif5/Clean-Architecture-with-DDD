using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Common;
using YourApp.Domain.Constants;

namespace YourApp.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                    return null;

                return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        public string? UserName
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                    return null;

                return user.FindFirst(ClaimTypes.Name)?.Value;
            }
        }

        public string? Email
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                    return null;

                return user.FindFirst(ClaimTypes.Email)?.Value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.Identity?.IsAuthenticated == true;
            }
        }

        public bool IsAdmin
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.IsInRole(Roles.SuperAdmin) == true;
            }
        }

        public bool IsInRole(string role)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.IsInRole(role) == true;
        }

        public HeaderData GetUserProfile()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            var claims = user.Identities.FirstOrDefault()?.Claims?.ToList() ?? new List<Claim>();

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.FindFirstValue(ClaimTypes.Name);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var firstName = user.FindFirstValue(ClaimTypes.GivenName);
            var lastName = user.FindFirstValue(ClaimTypes.Surname);
            var roles = claims.Where(c => c.Type == ClaimTypes.Role).ToList();

            if (string.IsNullOrEmpty(userId))
                return null;

            return new HeaderData
            {
                UserId = new Guid(userId),
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Claims = claims,
                Roles = roles,
                CanCreate = HasPermission(Permissions.Claims.CanCreate),
                CanRead = HasPermission(Permissions.Claims.CanRead),
                CanUpdate = HasPermission(Permissions.Claims.CanUpdate),
                CanDelete = HasPermission(Permissions.Claims.CanDelete),
                CanExport = HasPermission(Permissions.Claims.CanExport)
            };
        }

        public async Task<HeaderData> GetUserProfileAsync()
        {
            return await Task.FromResult(GetUserProfile());
        }

        private bool HasPermission(string permissionType)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;

            // SuperAdmin has all permissions
            if (user.IsInRole(Roles.SuperAdmin))
                return true;

            // Check if user has any claim with the permission type
            return user.Claims.Any(c =>
                c.Type == Roles.AcademicManager ||
                c.Type == Roles.ManageRole ||
                c.Value == permissionType);
        }
    }
}