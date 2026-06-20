using System.Security.Claims;
using System.Text.Json.Serialization;

namespace YourApp.Domain.Common
{
    public class HeaderData : Permission
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // ✅ Use JsonIgnore to prevent serialization of Claims
        [JsonIgnore]
        public List<Claim>? Claims { get; set; }

        // ✅ Use JsonIgnore to prevent serialization of Roles
        [JsonIgnore]
        public List<Claim> Roles { get; set; } = new();

        // ✅ Add a serializable version of roles
        public List<string> RoleNames => Roles.Select(r => r.Value).ToList();

        // ✅ Add a serializable version of claims (key-value pairs)
        public Dictionary<string, string> ClaimValues => Claims?
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => string.Join(",", g.Select(c => c.Value)))
            ?? new Dictionary<string, string>();

        public string GetFullName()
        {
            return FirstName is null
                || LastName is null
                || string.IsNullOrEmpty(FirstName)
                || string.IsNullOrEmpty(LastName)
                    ? UserName
                    : $"{FirstName} {LastName}";
        }
    }
}