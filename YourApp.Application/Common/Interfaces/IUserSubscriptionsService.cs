namespace YourApp.Application.Common.Interfaces
{
    public interface IUserSubscriptionsService
    {
        Task<bool> IsPlanExpired(Guid userId);
        Task<bool> HasPermissionAsync(Guid userId, string permission);
    }
}