using Microsoft.Extensions.Logging;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Common.Models;

namespace YourApp.Infrastructure.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ILogger<ActivityService> _logger;
        private static readonly List<ActivityModel> _activities = new();

        public ActivityService(ILogger<ActivityService> logger)
        {
            _logger = logger;
        }

        public async Task SaveActivityLog(ActivityModel activity)
        {
            try
            {
                // In a real implementation, you would save to a database
                _activities.Add(activity);

                // Log to console/logger
                _logger.LogInformation(
                    "Activity Log: UserId={UserId}, Url={RequestUrl}, StatusCode={StatusCode}, Date={ActivityDate}",
                    activity.UserId,
                    activity.RequestUrl,
                    activity.StatusCode,
                    activity.ActivityDate
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save activity log");
            }
        }

        public async Task<IEnumerable<ActivityModel>> GetActivitiesByUserIdAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _activities.Where(a => a.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(a => a.ActivityDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.ActivityDate <= toDate.Value);

            return await Task.FromResult(query.OrderByDescending(a => a.ActivityDate));
        }

        public async Task<IEnumerable<ActivityModel>> GetActivitiesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await Task.FromResult(
                _activities
                    .Where(a => a.ActivityDate >= fromDate && a.ActivityDate <= toDate)
                    .OrderByDescending(a => a.ActivityDate)
            );
        }

        public async Task<IEnumerable<ActivityModel>> GetLatestActivitiesAsync(int count)
        {
            return await Task.FromResult(
                _activities
                    .OrderByDescending(a => a.ActivityDate)
                    .Take(count)
            );
        }

        public async Task<IEnumerable<ActivityModel>> GetActivitiesByRequestUrlAsync(string requestUrl)
        {
            return await Task.FromResult(
                _activities
                    .Where(a => a.RequestUrl.Contains(requestUrl))
                    .OrderByDescending(a => a.ActivityDate)
            );
        }
    }
}