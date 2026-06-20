using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.DTOs;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Application.Users.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IIdentityService identityService,
            IJwtService jwtService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _identityService = identityService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate refresh token
                var (isValid, userId) = await _jwtService.ValidateRefreshTokenAsync(request.RefreshToken);

                if (!isValid || string.IsNullOrEmpty(userId))
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "Invalid or expired refresh token" }
                    };
                }

                // Get user
                var user = await _identityService.GetUserByIdAsync(userId);
                if (user == null || !user.CanLogin())
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "User not found or account is inactive" }
                    };
                }

                // Revoke old refresh token
                await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken, "IP_ADDRESS");

                // Generate new tokens
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id.ToString());

                // Get user roles
                var roles = await _identityService.GetUserRolesAsync(user);

                _logger.LogInformation("Tokens refreshed for user: {Email}", user.Email);

                return new AuthResponse
                {
                    IsSuccess = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new AuthResponse.UserInfo
                    {
                        Id = user.Id.ToString(),
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.GetFullName(),
                        UserType = user.UserType.ToString(),
                        Status = user.Status.ToString(),
                        Roles = roles.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Errors = new[] { "An error occurred while refreshing tokens" }
                };
            }
        }
    }
}