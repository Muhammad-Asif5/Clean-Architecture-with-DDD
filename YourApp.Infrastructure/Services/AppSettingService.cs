using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Settings;

namespace YourApp.Infrastructure.Services
{
    public class AppSettingService : IAppSettingService
    {
        private readonly AppSetting _appSettings;

        public AppSettingService(AppSetting appSettings)
        {
            _appSettings = appSettings;
        }

        public AppSetting GetAppSettings() => _appSettings;
        public JwtSettings GetJwtSettings() => _appSettings.Jwt;
        public IdentitySettings GetIdentitySettings() => _appSettings.IdentitySettings;
    }
}