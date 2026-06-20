using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourApp.Domain.Common;
using YourApp.Domain.Entities;
using YourApp.Domain.Enums;
using YourApp.Domain.Interfaces;

namespace YourApp.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>,
        IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        IQueryable<Product> IApplicationDbContext.Products => Products;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditableEntities()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            IdentityTable(builder);

            // ✅ Configure ApplicationUser with proper mapping
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.UserType)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                // ✅ Map IsEmailConfirmed to the existing EmailConfirmed column
                entity.Property(e => e.IsEmailConfirmed)
                    .HasColumnName("EmailConfirmed");

                // ✅ Configure LockoutEnabled - map to existing column
                entity.Property(e => e.LockoutEnabled)
                    .HasColumnName("LockoutEnabled");

                // ✅ Explicitly ignore the base EmailConfirmed property
                // This prevents EF from creating a separate column
                entity.Ignore(e => e.EmailConfirmed);

                entity.HasMany(e => e.RefreshTokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken configuration
            builder.Entity<RefreshToken>().ToTable("RefreshToken", "U");
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token)
                    .HasMaxLength(512)
                    .IsRequired();
                entity.Property(e => e.RevokedByIp)
                    .HasMaxLength(45);
                entity.Property(e => e.CreatedByIp)
                    .HasMaxLength(45);
                entity.Property(e => e.ReplacedByToken)
                    .HasMaxLength(512);
                entity.Property(e => e.ApplicationUserId)
                    .IsRequired()
                    .HasMaxLength(36);

                entity.HasIndex(e => e.Token)
                    .IsUnique();

                entity.HasIndex(e => e.ExpiryDate);
            });

            // Product configuration
            builder.Entity<Product>().ToTable("Product", "U");
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.Price)
                    .HasPrecision(18, 2)
                    .IsRequired();
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
            });
        }

        private void IdentityTable(ModelBuilder modelBuilder)
        {
            // Convert AspNetUser Table to Users
            modelBuilder.Entity<ApplicationUser>()
                .ToTable("Users", "u")
                .Property(e => e.Id).HasColumnName("UserId");

            // Convert AspNetRole Table to Role
            modelBuilder.Entity<IdentityRole<Guid>>()
                .ToTable("Roles", "u");

            // Convert AspNetUsersRoles to UserRole
            modelBuilder.Entity<IdentityUserRole<Guid>>()
                .ToTable("UserRoles", "u");

            // Convert AspNetUsersToken to UserToken
            modelBuilder.Entity<IdentityUserToken<Guid>>()
                .ToTable("UserToken", "u");

            // Convert AspNetUserClaims to UserClaim
            modelBuilder.Entity<IdentityUserClaim<Guid>>()
                .ToTable("UserClaims", "u");

            // Convert AspNetUserLogins to UserLogin
            modelBuilder.Entity<IdentityUserLogin<Guid>>()
                .ToTable("UserLogins", "u");

            // Convert AspNetRoleClaim to RoleClaim
            modelBuilder.Entity<IdentityRoleClaim<Guid>>()
                .ToTable("RoleClaim", "u");
        }

        public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Domain.Constants.Roles.SuperAdmin.ToString()))
            {
                // Seed Default User
                var adminUser = new ApplicationUser(
                    "virtual", "software", "virtual.software.technology@gmail.com", "vst",
                    UserType.SuperAdmin,
                    UserStatus.Active);

                adminUser.PhoneNumber = "03475414625";
                adminUser.IsEmailConfirmed = true;
                adminUser.LockoutEnabled = false;

                string pwdAdmin = "virtual.software.technology@gmail.com";

                var newAdminUser = await userManager.CreateAsync(adminUser, pwdAdmin);

                if (newAdminUser.Succeeded)
                {
                    // After creation, ensure LockoutEnabled is false
                    adminUser.LockoutEnabled = false;
                    await userManager.UpdateAsync(adminUser);

                    // Create roles...
                    await roleManager.CreateAsync(new IdentityRole<Guid>(Domain.Constants.Roles.SuperAdmin));
                    await roleManager.CreateAsync(new IdentityRole<Guid>(Domain.Constants.Roles.ManageRole));
                    await roleManager.CreateAsync(new IdentityRole<Guid>(Domain.Constants.Roles.Teacher));
                    await roleManager.CreateAsync(new IdentityRole<Guid>(Domain.Constants.Roles.Product));

                    await userManager.AddToRoleAsync(adminUser, Domain.Constants.Roles.SuperAdmin);
                    await userManager.AddToRoleAsync(adminUser, Domain.Constants.Roles.ManageRole);
                }
            }
        }
    }
}