using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Entities;
using YourApp.Domain.Settings;
using YourApp.Infrastructure.Persistence.Context;
using static YourApp.Domain.Constants.Permissions;

namespace YourApp.Infrastructure.Identity
{
    public class JwtService : IJwtService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;

        public JwtService(
            ApplicationDbContext context,
            IAppSettingService appSettingService,
            ILogger<JwtService> logger)
        {
            _context = context;
            _jwtSettings = appSettingService.GetJwtSettings();
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var claims = await GetClaimsAsync(user);
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer ?? _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience ?? _jwtSettings.Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenValidityInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress = null)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
                throw new ArgumentException("Invalid user ID format", nameof(userId));

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiryDays = _jwtSettings.RefreshTokenValidityInDays;

            var refreshToken = new RefreshToken(
                userIdGuid,
                token,
                DateTime.UtcNow.AddDays(expiryDays),
                ipAddress);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token generated for user {UserId}", userId);
            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken == null)
                return false;

            refreshToken.Revoke(ipAddress);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked: {Token}", token);
            return true;
        }

        public async Task<(bool IsValid, string UserId)> ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken == null || !refreshToken.IsActive())
                return (false, null);

            return (true, refreshToken.ApplicationUserId.ToString());
        }

        private async Task<List<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.GetFullName()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("UserType", user.UserType.ToString()),
                new Claim("Status", user.Status.ToString()),
                new Claim("IsEmailConfirmed", user.IsEmailConfirmed.ToString())
            };

            var userClaims = await GetUserClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await GetUserRolesAsync(user);
            var roleClaims = await GetRolesClaimAsync(userRoles);
            claims.AddRange(roleClaims);

            return claims;
        }

        private async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            var userManager = _context.GetService<UserManager<ApplicationUser>>();
            return await userManager.GetRolesAsync(user);
        }

        private async Task<IList<Claim>> GetRolesClaimAsync(IList<string> userRoles)
        {
            var claims = new List<Claim>();
            var roleManager = _context.GetService<RoleManager<IdentityRole<Guid>>>();
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));

                var role = await roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    claims.AddRange(roleClaims);
                }
            }

            return claims;
        }

        private async Task<IList<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            var userManager = _context.GetService<UserManager<ApplicationUser>>();
            var claims = await userManager.GetClaimsAsync(user); 
            return claims;
        }
    }
}