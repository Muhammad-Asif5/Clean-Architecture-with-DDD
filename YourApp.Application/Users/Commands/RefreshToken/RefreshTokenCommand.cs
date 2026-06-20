using MediatR;
using YourApp.Application.Common.DTOs;

namespace YourApp.Application.Users.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthResponse>
    {
        public string RefreshToken { get; set; }
    }
}