using Microsoft.AspNetCore.Identity;
using YourApp.Domain.Enums;
using YourApp.Domain.Exceptions;

namespace YourApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public UserType UserType { get; private set; }
        public UserStatus Status { get; set; }
        public string? ProfilePictureUrl { get; private set; }
        public DateTime? LastLoginDate { get; private set; }
        public DateTime? PasswordChangedDate { get; private set; }
        public bool IsTwoFactorEnabled { get; private set; }
        public Guid? CreatedBy { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; private set; }

        // ✅ Use IsEmailConfirmed as the main property
        public bool IsEmailConfirmed { get; set; }

        // ✅ Don't override EmailConfirmed - we'll configure it via Fluent API
        // Remove the override to avoid conflicts

        private ApplicationUser()
        {
            RefreshTokens = new List<RefreshToken>();
            Id = Guid.NewGuid();
            LockoutEnabled = true;
            IsEmailConfirmed = false;
        }

        public ApplicationUser(
            string firstName,
            string lastName,
            string email,
            string userName,
            UserType userType = UserType.Academic,
            UserStatus status = UserStatus.PendingApproval) : base(userName)
        {
            ValidateNames(firstName, lastName);

            Id = Guid.NewGuid();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email;
            UserName = userName;
            UserType = userType;
            Status = status;
            CreatedAt = DateTime.UtcNow;
            RefreshTokens = new List<RefreshToken>();

            IsTwoFactorEnabled = false;
            AccessFailedCount = 0;
            LockoutEnabled = true;
            LockoutEnd = null;
            IsEmailConfirmed = false;
        }

        public void UpdateProfile(string firstName, string lastName, string? profilePictureUrl = null)
        {
            ValidateNames(firstName, lastName);
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            ProfilePictureUrl = profilePictureUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateUserType(UserType newUserType)
        {
            UserType = newUserType;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(UserStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordLogin()
        {
            LastLoginDate = DateTime.UtcNow;
            ResetFailedLoginAttempts();
        }

        public void RecordPasswordChange()
        {
            PasswordChangedDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ConfirmEmail()
        {
            IsEmailConfirmed = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EnableTwoFactor()
        {
            IsTwoFactorEnabled = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DisableTwoFactor()
        {
            IsTwoFactorEnabled = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EnableLockout()
        {
            LockoutEnabled = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DisableLockout()
        {
            LockoutEnabled = false;
            ResetFailedLoginAttempts();
            UpdatedAt = DateTime.UtcNow;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}".Trim();
        }

        public bool CanLogin()
        {
            if (!IsEmailConfirmed)
            {
                return false;
            }

            if (LockoutEnabled && IsLockedOut())
            {
                return false;
            }

            return Status.CanLogin();
        }

        public bool IsLockedOut()
        {
            return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;
        }

        public void ResetFailedLoginAttempts()
        {
            AccessFailedCount = 0;
            LockoutEnd = null;
            if (Status == UserStatus.Locked)
            {
                Status = UserStatus.Active;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public int GetRemainingAttempts(int maxAttempts)
        {
            if (!LockoutEnabled)
            {
                return int.MaxValue;
            }
            return Math.Max(0, maxAttempts - AccessFailedCount);
        }

        public bool IsAccountLocked()
        {
            return LockoutEnabled && (IsLockedOut() || Status == UserStatus.Locked);
        }

        public int GetLockoutMinutesRemaining()
        {
            if (!LockoutEnabled || !LockoutEnd.HasValue)
                return 0;

            var remaining = (int)Math.Ceiling((LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes);
            return Math.Max(0, remaining);
        }

        public bool ShouldApplyLockout(bool globalLockoutOnFailure)
        {
            return globalLockoutOnFailure && LockoutEnabled;
        }

        private static void ValidateNames(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name is required");

            if (firstName.Length > 50)
                throw new DomainException("First name cannot exceed 50 characters");

            if (lastName.Length > 50)
                throw new DomainException("Last name cannot exceed 50 characters");
        }
    }
}