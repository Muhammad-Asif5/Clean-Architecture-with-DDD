using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using YourApp.Application.Authorization;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Services;
using YourApp.Domain.Constants;
using YourApp.Domain.Entities;
using YourApp.Domain.Interfaces;
using YourApp.Domain.Settings;
using YourApp.Infrastructure.Identity;
using YourApp.Infrastructure.Persistence.Context;
using YourApp.Infrastructure.Repositories;
using YourApp.Infrastructure.Services;

namespace YourApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ✅ Load AppSettings from configuration
            var appSetting = configuration.GetSection("AppSetting").Get<AppSetting>();
            services.AddSingleton(appSetting);

            // Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection") ??
                    configuration["SqlConnection:ConnectionStrings:Local:InventoryDB"],
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // ✅ Identity with AppSettings
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                var identitySettings = appSetting.IdentitySettings;
                var password = identitySettings.Password;
                var user = identitySettings.User;
                var lockout = identitySettings.Lockout;
                var signIn = identitySettings.SignIn;

                // Password Settings
                options.Password.RequiredLength = password.RequiredLength;
                options.Password.RequiredUniqueChars = password.RequiredUniqueChars;
                options.Password.RequireUppercase = password.RequireUppercase;
                options.Password.RequireLowercase = password.RequireLowercase;
                options.Password.RequireDigit = password.RequireDigit;
                options.Password.RequireNonAlphanumeric = password.RequireNonAlphanumeric;

                // User Settings
                options.User.RequireUniqueEmail = user.RequireUniqueEmail;
                options.User.AllowedUserNameCharacters = user.AllowedUserNameCharacters;

                // Lockout Settings
                options.Lockout.AllowedForNewUsers = lockout.AllowedForNewUsers;
                options.Lockout.MaxFailedAccessAttempts = lockout.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockout.UserLockoutTimeInMinutes);

                // SignIn Settings
                options.SignIn.RequireConfirmedEmail = signIn.RequireConfirmedEmail;
                options.SignIn.RequireConfirmedPhoneNumber = signIn.RequireConfirmedPhoneNumber;
                options.SignIn.RequireConfirmedAccount = signIn.RequireConfirmedAccount;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ✅ JWT Authentication with AppSettings
            var jwtSettings = appSetting.Jwt;
            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience ?? jwtSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError("Authentication failed: {Exception}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });

            // ✅ Register HttpContextAccessor
            services.AddHttpContextAccessor();
            // ✅ Register CurrentUserService
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // ✅ Register ActivityService
            services.AddScoped<IActivityService, ActivityService>();


            #region Permission / Policy

            // ✅ Register Authorization Handlers
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            // ✅ Register Authorization Policies from Permissions
            services.AddAuthorization(options =>
            {
                foreach (var controller in Permissions.ControllerPermissions)
                {
                    foreach (var action in controller.Value)
                    {
                        var role = action.Split(".")[0];
                        options.AddPolicy(action, policy =>
                        {
                            policy.Requirements.Add(new PermissionRequirement(
                                action,
                                role,
                                Roles.SuperAdmin,
                                Roles.ManageRole,
                                Roles.AcademicManager));
                        });
                    }
                }
            });

            #endregion

            // Register Services
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IAppSettingService, AppSettingService>();
            services.AddScoped<IUserSubscriptionsService, UserSubscriptionsService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}