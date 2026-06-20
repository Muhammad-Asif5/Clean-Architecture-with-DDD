using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Exceptions;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Constants;

namespace YourApp.Application.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserSubscriptionsService _userSubscriptionsService;
        private readonly ILogger<PermissionHandler> _logger;

        public PermissionHandler(
            IUserSubscriptionsService userSubscriptionsService,
            ILogger<PermissionHandler> logger)
        {
            _userSubscriptionsService = userSubscriptionsService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var user = context.User;

            // Make sure user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("User is not authenticated");
                return;
            }

            // Get userId from JWT claims
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return;
            }

            // Check if user is SuperAdmin
            if (user.IsInRole(Roles.SuperAdmin))
            {
                _logger.LogInformation("User {UserId} is SuperAdmin, granting access", userId);
                context.Succeed(requirement);
                return;
            }

            // Check if user has AcademicManager or ManageRole
            if (user.IsInRole(Roles.AcademicManager) || user.IsInRole(Roles.ManageRole))
            {
                await VerifyPlanExpirationAndSucceedAsync(context, requirement, userId);
                return;
            }

            // For other roles, check if the user has the required role and permission
            if (requirement.Roles.Any(role => user.IsInRole(role)))
            {
                await VerifyPlanExpirationAndCheckPermissionAsync(context, requirement, user, userId);
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have required roles: {Roles}",
                    userId, string.Join(", ", requirement.Roles));
                throw new PermissionDeniedException("You do not have the required permissions to perform this action.");
            }
        }

        private async Task VerifyPlanExpirationAndSucceedAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement,
            string userId)
        {
            var userGuid = new Guid(userId);
            var isExpired = await _userSubscriptionsService.IsPlanExpired(userGuid);

            if (isExpired)
            {
                _logger.LogWarning("User {UserId} has an expired plan", userId);
                throw new PlanExpiredException("Your subscription plan has expired.");
            }

            _logger.LogInformation("User {UserId} granted access to {Permission} with AcademicManager/ManageRole",
                userId, requirement.Permission);
            context.Succeed(requirement);
        }

        private async Task VerifyPlanExpirationAndCheckPermissionAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement,
            ClaimsPrincipal user,
            string userId)
        {
            var userGuid = new Guid(userId);
            var isExpired = await _userSubscriptionsService.IsPlanExpired(userGuid);

            if (isExpired)
            {
                _logger.LogWarning("User {UserId} has an expired plan", userId);
                throw new PlanExpiredException("Your subscription plan has expired.");
            }

            // Extract required permission and check user claims
            var requiredClaimType = requirement.Permission;
            var requiredClaimValue = requirement.Permission.Split('.')[1];

            if (user.HasClaim(c => c.Type == requiredClaimType && c.Value == requiredClaimValue))
            {
                _logger.LogInformation("User {UserId} granted access to {Permission}",
                    userId, requirement.Permission);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have required permission: {Permission}",
                    userId, requirement.Permission);
                throw new PermissionDeniedException("You do not have the required permissions to perform this action.");
            }
        }
    }
}