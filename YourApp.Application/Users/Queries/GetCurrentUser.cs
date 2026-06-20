using MediatR;
using YourApp.Application.Common.DTOs;

namespace YourApp.Application.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<AuthResponse.UserInfo>
    {
        public string UserId { get; set; }
    }
}