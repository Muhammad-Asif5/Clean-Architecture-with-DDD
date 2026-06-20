using YourApp.Application.Common.Models;

namespace YourApp.Application.Common.Interfaces
{
    public interface IActivityService
    {
        Task SaveActivityLog(ActivityModel activity);
        Task<IEnumerable<ActivityModel>> GetActivitiesByUserIdAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<ActivityModel>> GetActivitiesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<ActivityModel>> GetLatestActivitiesAsync(int count);
        Task<IEnumerable<ActivityModel>> GetActivitiesByRequestUrlAsync(string requestUrl);
    }
}