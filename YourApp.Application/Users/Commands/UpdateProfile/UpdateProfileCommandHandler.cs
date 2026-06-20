using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.DTOs;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Exceptions;

namespace YourApp.Application.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, AuthResponse.UserInfo>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateProfileCommandHandler> _logger;

        public UpdateProfileCommandHandler(
            IIdentityService identityService,
            ILogger<UpdateProfileCommandHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<AuthResponse.UserInfo> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _identityService.GetUserByIdAsync(request.UserId);
                if (user == null)
                    throw new DomainException("User not found");

                user.UpdateProfile(
                    request.FirstName,
                    request.LastName,
                    request.ProfilePictureUrl);

                var (succeeded, errors) = await _identityService.UpdateUserAsync(user);
                if (!succeeded)
                    throw new DomainException(string.Join(", ", errors));

                var roles = await _identityService.GetUserRolesAsync(user);

                _logger.LogInformation("User profile updated: {UserId}", request.UserId);

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
                _logger.LogError(ex, "Error updating user profile: {UserId}", request.UserId);
                throw;
            }
        }
    }
}