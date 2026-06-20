namespace YourApp.API.Controllers
{
    public class LogoutCommand
    {
        public string RefreshToken { get; set; }
    }

    // ✅ This is now in Application/Users/Commands/RevokeToken
    // public class RevokeTokenCommand : IRequest<bool> { ... }

    // ✅ This is now in Application/Users/Queries/GetCurrentUser
    // public class GetCurrentUserQuery : IRequest<AuthResponse.UserInfo> { ... }
}