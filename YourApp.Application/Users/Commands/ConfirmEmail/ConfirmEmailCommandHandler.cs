using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.DTOs;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Application.Users.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<ConfirmEmailCommandHandler> _logger;

        public ConfirmEmailCommandHandler(
            IIdentityService identityService,
            IJwtService jwtService,
            ILogger<ConfirmEmailCommandHandler> logger)
        {
            _identityService = identityService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get user
                var user = await _identityService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "User not found" }
                    };
                }

                // Confirm email
                var (succeeded, errors) = await _identityService.ConfirmEmailAsync(user, request.Token);

                if (!succeeded)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = errors
                    };
                }

                // ✅ Generate tokens for the user
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id.ToString());

                var roles = await _identityService.GetUserRolesAsync(user);

                _logger.LogInformation("Email confirmed successfully for user: {Email}", user.Email);

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
                        Roles = roles.ToList(),
                        IsEmailConfirmed = true
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user: {UserId}", request.UserId);
                return new AuthResponse
                {
                    IsSuccess = false,
                    Errors = new[] { "An error occurred while confirming email" }
                };
            }
        }
    }
}