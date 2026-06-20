using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Application.Users.Commands.RevokeToken
{
    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
    {
        private readonly IJwtService _jwtService;
        private readonly ILogger<RevokeTokenCommandHandler> _logger;

        public RevokeTokenCommandHandler(IJwtService jwtService, ILogger<RevokeTokenCommandHandler> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return false;

            var result = await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken, "IP_ADDRESS");

            if (result)
            {
                _logger.LogInformation("Refresh token revoked successfully");
            }

            return result;
        }
    }
}