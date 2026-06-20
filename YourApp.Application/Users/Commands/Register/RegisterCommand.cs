using MediatR;
using YourApp.Application.Common.DTOs;
using YourApp.Domain.Enums;

namespace YourApp.Application.Users.Commands.Register
{
    public class RegisterCommand : IRequest<AuthResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public UserType UserType { get; set; } = UserType.Academic;
    }
}