using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.DTOs;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Enums;
using YourApp.Domain.Settings;

namespace YourApp.Application.Users.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        private readonly IAppSettingService _appSettingService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IIdentityService identityService,
            IJwtService jwtService,
            IAppSettingService appSettingService,
            ILogger<LoginCommandHandler> logger)
        {
            _identityService = identityService;
            _jwtService = jwtService;
            _appSettingService = appSettingService;
            _logger = logger;
        }

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ Get lockout settings
                var identitySettings = _appSettingService.GetIdentitySettings();
                var lockoutSettings = identitySettings?.Lockout
                    ?? new LockoutSettings
                    {
                        AllowedForNewUsers = true,
                        LockoutOnFailure = true,
                        MaxFailedAccessAttempts = 5,
                        UserLockoutTimeInMinutes = 15
                    };

                var maxAttempts = lockoutSettings.MaxFailedAccessAttempts;
                var lockoutMinutes = lockoutSettings.UserLockoutTimeInMinutes;
                var globalLockoutOnFailure = lockoutSettings.LockoutOnFailure;

                // Get user by email
                var user = await _identityService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User not found with email {Email}", request.Email);
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "Invalid email or password" }
                    };
                }

                // ✅ Check if account is locked (only if lockout is enabled globally and for this user)
                if (globalLockoutOnFailure && user.IsAccountLocked())
                {
                    var remainingMinutes = user.GetLockoutMinutesRemaining();

                    if (remainingMinutes > 0)
                    {
                        return new AuthResponse
                        {
                            IsSuccess = false,
                            Errors = new[]
                            {
                                $"Your account is locked. Please try again after {remainingMinutes} minute(s)."
                            },
                            LockoutMinutesRemaining = remainingMinutes
                        };
                    }
                    else
                    {
                        // Lockout expired - reset attempts
                        user.ResetFailedLoginAttempts();
                        await _identityService.UpdateUserAsync(user);
                    }
                }

                // Check email confirmation
                if (!user.IsEmailConfirmed)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "Please confirm your email address before logging in. Check your inbox for the confirmation link." }
                    };
                }

                // Check if user can login (status check)
                if (!user.CanLogin())
                {
                    var statusMessage = user.Status switch
                    {
                        UserStatus.Inactive => "Your account is inactive",
                        UserStatus.Suspended => "Your account has been suspended",
                        UserStatus.Locked => "Your account has been locked",
                        UserStatus.PendingApproval => "Your account is pending approval",
                        _ => "Your account cannot be accessed at this time"
                    };

                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { statusMessage }
                    };
                }

                // Check password
                var passwordValid = await _identityService.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    // ✅ Determine if lockout should be applied
                    var shouldApplyLockout = user.ShouldApplyLockout(globalLockoutOnFailure);

                    if (shouldApplyLockout)
                    {
                        // Increment failed attempts
                        user.AccessFailedCount++;

                        // Check if lockout should be applied
                        if (user.AccessFailedCount >= maxAttempts)
                        {
                            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes);
                            user.UpdateStatus(UserStatus.Locked);
                        }
                    }
                    else
                    {
                        // ✅ If lockout is disabled (globally or per-user), don't increment attempts
                        // Just log the failed attempt without incrementing
                        _logger.LogWarning("Login attempt failed for user {Email} but lockout is disabled. GlobalLockoutOnFailure: {GlobalLockout}, UserLockoutEnabled: {UserLockoutEnabled}",
                            request.Email, globalLockoutOnFailure, user.LockoutEnabled);
                    }

                    await _identityService.UpdateUserAsync(user);

                    // ✅ Determine error message based on lockout state
                    string errorMessage;
                    if (!globalLockoutOnFailure)
                    {
                        // ✅ Global lockout is disabled - no lockout messages
                        errorMessage = "Invalid credentials.";
                    }
                    else if (!user.LockoutEnabled)
                    {
                        // ✅ User has lockout disabled - no lockout messages
                        errorMessage = "Invalid credentials.";
                    }
                    else
                    {
                        // ✅ Lockout is enabled - show remaining attempts
                        var remainingAttempts = user.GetRemainingAttempts(maxAttempts);
                        if (remainingAttempts > 0)
                        {
                            errorMessage = $"Invalid credentials. {remainingAttempts} attempt(s) remaining.";
                        }
                        else
                        {
                            errorMessage = "Your account has been locked due to multiple failed login attempts.";
                        }
                    }

                    var remainingAttemptsForResponse = (globalLockoutOnFailure && user.LockoutEnabled)
                        ? user.GetRemainingAttempts(maxAttempts)
                        : (int?)null;

                    _logger.LogWarning("Login attempt failed: Invalid password for user {Email}. GlobalLockoutOnFailure: {GlobalLockout}, UserLockoutEnabled: {UserLockoutEnabled}, AccessFailedCount: {AccessFailedCount}",
                        request.Email, globalLockoutOnFailure, user.LockoutEnabled, user.AccessFailedCount);

                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { errorMessage },
                        RemainingAttempts = remainingAttemptsForResponse
                    };
                }

                // ✅ Check if account was locked but password was correct
                if (globalLockoutOnFailure && user.IsAccountLocked())
                {
                    var remainingMinutes = user.GetLockoutMinutesRemaining();

                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Errors = new[]
                        {
                            $"Your account is locked. Please try again after {remainingMinutes} minute(s)."
                        },
                        LockoutMinutesRemaining = remainingMinutes
                    };
                }

                // Record successful login (resets failed attempts)
                user.RecordLogin();
                await _identityService.UpdateUserAsync(user);

                // Get user roles
                var roles = await _identityService.GetUserRolesAsync(user);

                // Generate tokens
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync(
                    user.Id.ToString(),
                    "IP_ADDRESS_HERE"
                );

                _logger.LogInformation("User logged in successfully: {Email}", request.Email);

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
                    },
                    RemainingAttempts = null,
                    LockoutMinutesRemaining = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new AuthResponse
                {
                    IsSuccess = false,
                    Errors = new[] { "An error occurred during login" }
                };
            }
        }
    }
}