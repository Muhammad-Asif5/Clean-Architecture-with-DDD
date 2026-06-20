using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.DTOs;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Constants;
using YourApp.Domain.Entities;
using YourApp.Domain.Enums;
using YourApp.Domain.Settings;

namespace YourApp.Application.Users.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IdentitySettings _identitySettings;
        private readonly IEmailService _emailService;

        public RegisterCommandHandler(
            IIdentityService identityService,
            IJwtService jwtService,
            IEmailService emailService,
            IAppSettingService appSettingService,
            ILogger<RegisterCommandHandler> logger)
        {
            _identityService = identityService;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
            _identitySettings = appSettingService.GetIdentitySettings();
        }

        public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if email already exists
                var existingEmail = await _identityService.GetUserByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "A user with this email already exists" }
                    };
                }

                // Check if username already exists
                var existingUsername = await _identityService.GetUserByUsernameAsync(request.UserName);
                if (existingUsername != null)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "A user with this username already exists" }
                    };
                }

                // Create user
                var user = new ApplicationUser(
                    request.FirstName.Trim(),
                    request.LastName.Trim(),
                    request.Email.Trim(),
                    request.UserName.Trim(),
                    request.UserType,
                    UserStatus.Active);

                user.EmailConfirmed = _identitySettings.SignIn.RequireConfirmedEmail;

                // Create user in Identity
                var (succeeded, errors) = await _identityService.CreateUserAsync(user, request.Password);

                if (!succeeded)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = errors
                    };
                }

                // Add to role
                await _identityService.AddToRoleAsync(user, new List<string>
                {
                    Roles.ManageRole,
                    Roles.AcademicManager
                });

                // ✅ Generate email confirmation token
                var (tokenSucceeded, confirmationToken) = await _identityService.GenerateEmailConfirmationTokenAsync(user);
                if (tokenSucceeded)
                {
                    // ✅ Send confirmation email
                    await _emailService.SendEmailConfirmationAsync(user, confirmationToken);
                    _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to generate confirmation token for {Email}", user.Email);
                }

                //// Generate tokens for auto-login after registration
                //var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                //var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id.ToString());

                // Get roles
                var roles = await _identityService.GetUserRolesAsync(user);

                return new AuthResponse
                {
                    IsSuccess = true,
                    AccessToken = null, // No token until email is confirmed
                    RefreshToken = null,
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
                _logger.LogError(ex, "Error registering user: {Email}", request.Email);
                return new AuthResponse
                {
                    IsSuccess = false,
                    Errors = new[] { "An error occurred while registering the user" }
                };
            }
        }
    }
}