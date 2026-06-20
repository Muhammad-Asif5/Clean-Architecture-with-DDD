namespace YourApp.Domain.Settings
{
    public class AppSetting
    {
        public IdentitySettings IdentitySettings { get; set; }
        public JwtSettings Jwt { get; set; }
        public bool IsSendEmail { get; set; }
    }

    public class IdentitySettings
    {
        public PasswordSettings Password { get; set; }
        public SignInSettings SignIn { get; set; }
        public UserSettings User { get; set; }
        public LockoutSettings Lockout { get; set; }
    }

    public class PasswordSettings
    {
        public int RequiredLength { get; set; } = 6;
        public int RequiredUniqueChars { get; set; } = 1;
        public bool RequireNonAlphanumeric { get; set; } = false;
        public bool RequireUppercase { get; set; } = false;
        public bool RequireLowercase { get; set; } = false;
        public bool RequireDigit { get; set; } = false;
    }

    public class SignInSettings
    {
        public bool RequireConfirmedEmail { get; set; } = false;
        public bool RequireConfirmedPhoneNumber { get; set; } = false;
        public bool RequireConfirmedAccount { get; set; } = false;
    }

    public class UserSettings
    {
        public bool RequireUniqueEmail { get; set; } = true;
        public string AllowedUserNameCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    }

    public class LockoutSettings
    {
        public bool AllowedForNewUsers { get; set; } = true;
        public bool LockoutOnFailure { get; set; } = true;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int UserLockoutTimeInMinutes { get; set; } = 5;
    }
}