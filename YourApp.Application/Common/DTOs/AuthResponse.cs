namespace YourApp.Application.Common.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; }
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; }
        public int? RemainingAttempts { get; set; } // ✅ Add this
        public int? LockoutMinutesRemaining { get; set; } // ✅ Add this

        public AuthResponse()
        {
            Errors = Array.Empty<string>();
        }

        public class UserInfo
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string UserType { get; set; }
            public string Status { get; set; }
            public List<string> Roles { get; set; }
            public bool IsEmailConfirmed { get; set; }

            public UserInfo()
            {
                Roles = new List<string>();
            }
        }
    }
}