using MediatR;
using YourApp.Application.Common.DTOs;

namespace YourApp.Application.Users.Commands.ConfirmEmail
{
    public class ConfirmEmailCommand : IRequest<AuthResponse>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}