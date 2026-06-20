using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Application.Users.Commands.UnlockAccount
{
    public class UnlockAccountCommandHandler : IRequestHandler<UnlockAccountCommand, UnlockAccountResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<UnlockAccountCommandHandler> _logger;

        public UnlockAccountCommandHandler(
            IIdentityService identityService,
            ILogger<UnlockAccountCommandHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<UnlockAccountResponse> Handle(UnlockAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _identityService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    return new UnlockAccountResponse
                    {
                        IsSuccess = false,
                        Errors = new[] { "User not found" }
                    };
                }

                // Reset failed attempts and unlock
                user.ResetFailedLoginAttempts();
                await _identityService.UpdateUserAsync(user);

                _logger.LogInformation("Account unlocked for user: {Email}", user.Email);

                return new UnlockAccountResponse
                {
                    IsSuccess = true,
                    UserId = user.Id.ToString(),
                    UserName = user.UserName,
                    Email = user.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking account for user: {UserId}", request.UserId);
                return new UnlockAccountResponse
                {
                    IsSuccess = false,
                    Errors = new[] { "An error occurred while unlocking the account" }
                };
            }
        }
    }
}