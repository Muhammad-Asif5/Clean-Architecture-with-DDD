namespace YourApp.Domain.Settings
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
        public int TokenValidityInMinutes { get; set; }
        public string Issuer { get; set; }
    }
}
