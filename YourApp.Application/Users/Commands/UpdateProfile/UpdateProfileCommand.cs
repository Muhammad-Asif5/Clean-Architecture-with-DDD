using MediatR;
using YourApp.Application.Common.DTOs;

namespace YourApp.Application.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommand : IRequest<AuthResponse.UserInfo>
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}