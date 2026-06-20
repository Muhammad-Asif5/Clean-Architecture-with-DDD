using MediatR;
using YourApp.Application.Common.DTOs;

namespace YourApp.Application.Users.Commands.Login
{
    public class LoginCommand : IRequest<AuthResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}