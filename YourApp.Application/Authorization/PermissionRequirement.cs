using Microsoft.AspNetCore.Authorization;

namespace YourApp.Application.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }
        public string[] Roles { get; }

        public PermissionRequirement(string permission, params string[] roles)
        {
            Permission = permission;
            Roles = roles;
        }
    }
}