using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.DTOs;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Application.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, AuthResponse.UserInfo>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetCurrentUserQueryHandler> _logger;

        public GetCurrentUserQueryHandler(
            IIdentityService identityService,
            ILogger<GetCurrentUserQueryHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<AuthResponse.UserInfo> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    _logger.LogWarning("GetCurrentUserQuery called with null or empty UserId");
                    return null;
                }

                _logger.LogInformation("Getting user profile for UserId: {UserId}", request.UserId);

                var user = await _identityService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return null;
                }

                var roles = await _identityService.GetUserRolesAsync(user);

                return new AuthResponse.UserInfo
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.GetFullName(),
                    UserType = user.UserType.ToString(),
                    Status = user.Status.ToString(),
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for UserId: {UserId}", request.UserId);
                throw;
            }
        }
    }
}