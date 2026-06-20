using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourApp.Domain.Common;
using YourApp.Domain.Constants;

namespace YourApp.API.Controllers
{
    [Authorize]
    public abstract class ApiControllerExtensions : ControllerBase
    {
        [NonAction]
        public HeaderData GetProfile()
        {
            var user = HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
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

        [NonAction]
        protected bool HasPermission(string permissionType)
        {
            var user = HttpContext?.User;
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

        [NonAction]
        protected bool IsSuperAdmin()
        {
            var user = HttpContext?.User;
            return user?.IsInRole(Roles.SuperAdmin) == true;
        }

        [NonAction]
        protected bool IsInRole(string role)
        {
            var user = HttpContext?.User;
            return user?.IsInRole(role) == true;
        }

        [NonAction]
        protected string GetCurrentUserId()
        {
            var user = HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [NonAction]
        protected string GetCurrentUserName()
        {
            var user = HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Name);
        }

        [NonAction]
        protected string GetCurrentUserEmail()
        {
            var user = HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Email);
        }
    }
}