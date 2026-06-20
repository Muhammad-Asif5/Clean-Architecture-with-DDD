namespace YourApp.Domain.Enums
{
    public enum UserStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Locked = 4,
        PendingApproval = 5
    }

    public static class UserStatusExtensions
    {
        public static bool CanLogin(this UserStatus status)
        {
            return status == UserStatus.Active;
        }

        public static bool IsSuspendedOrLocked(this UserStatus status)
        {
            return status == UserStatus.Suspended || status == UserStatus.Locked;
        }
    }
}