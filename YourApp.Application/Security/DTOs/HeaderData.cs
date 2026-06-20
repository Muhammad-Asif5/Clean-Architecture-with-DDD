using System.Security.Claims;

namespace YourApp.Application.Security.DTOs
{
    public class HeaderData : Permission
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<Claim>? Claims { get; set; }
        public List<Claim> Roles { get; set; }
        public string GetFullName()
        {
            return FirstName is null
                || LastName is null
                || string.IsNullOrEmpty(FirstName)
                || string.IsNullOrEmpty(LastName) ? UserName : $"{FirstName} {LastName}";
        }
    }
}
