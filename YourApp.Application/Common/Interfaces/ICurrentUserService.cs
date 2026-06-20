using YourApp.Domain.Common;

namespace YourApp.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        bool IsAdmin { get; }
        HeaderData GetUserProfile();
        Task<HeaderData> GetUserProfileAsync();
    }
}