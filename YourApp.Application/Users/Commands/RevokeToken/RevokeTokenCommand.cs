using MediatR;

namespace YourApp.Application.Users.Commands.RevokeToken
{
    public class RevokeTokenCommand : IRequest<bool>
    {
        public string RefreshToken { get; set; }
    }
}