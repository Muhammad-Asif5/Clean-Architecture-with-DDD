using YourApp.Domain.Entities;

namespace YourApp.Application.Common.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress = null);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);
        Task<(bool IsValid, string UserId)> ValidateRefreshTokenAsync(string token);
    }
}