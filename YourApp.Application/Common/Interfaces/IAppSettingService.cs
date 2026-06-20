using YourApp.Domain.Settings;

namespace YourApp.Application.Common.Interfaces
{
    public interface IAppSettingService
    {
        AppSetting GetAppSettings();
        JwtSettings GetJwtSettings();
        IdentitySettings GetIdentitySettings();
    }
}