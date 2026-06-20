namespace YourApp.Application.Common.DTOs
{
    public class AuthRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}