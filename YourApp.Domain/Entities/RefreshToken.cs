using YourApp.Domain.Common;

namespace YourApp.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsUsed { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? CreatedByIp { get; private set; }
        public DateTime? RevokedDate { get; private set; }

        // ✅ Change to Guid to match IdentityUser<Guid>.Id
        public Guid ApplicationUserId { get; private set; }
        public virtual ApplicationUser User { get; private set; }

        private RefreshToken() { }

        public RefreshToken(
            Guid userId,
            string token,
            DateTime expiryDate,
            string createdByIp = null)
        {
            ApplicationUserId = userId;
            Token = token;
            ExpiryDate = expiryDate;
            CreatedByIp = createdByIp;
            IsUsed = false;
            IsRevoked = false;
        }

        public void MarkAsUsed() => IsUsed = true;

        public void Revoke(string ipAddress, string replacedByToken = null)
        {
            IsRevoked = true;
            RevokedByIp = ipAddress;
            RevokedDate = DateTime.UtcNow;
            ReplacedByToken = replacedByToken;
        }

        public bool IsActive() => !IsRevoked && !IsUsed && ExpiryDate > DateTime.UtcNow;
    }
}