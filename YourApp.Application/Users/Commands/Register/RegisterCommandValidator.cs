using FluentValidation;
using YourApp.Application.Common.Interfaces;
using YourApp.Domain.Settings;

namespace YourApp.Application.Users.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private readonly PasswordSettings _passwordSettings;

        public RegisterCommandValidator(IAppSettingService appSettingService)
        {
            var identitySettings = appSettingService.GetIdentitySettings();
            _passwordSettings = identitySettings.Password;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z0-9._]+$").WithMessage("Username can only contain letters, numbers, dots, and underscores");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(_passwordSettings.RequiredLength)
                    .WithMessage($"Password must be at least {_passwordSettings.RequiredLength} characters");

            if (_passwordSettings.RequireDigit)
            {
                RuleFor(x => x.Password)
                    .Matches("[0-9]").WithMessage("Password must contain at least one number");
            }

            if (_passwordSettings.RequireUppercase)
            {
                RuleFor(x => x.Password)
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter");
            }

            if (_passwordSettings.RequireLowercase)
            {
                RuleFor(x => x.Password)
                    .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter");
            }

            if (_passwordSettings.RequireNonAlphanumeric)
            {
                RuleFor(x => x.Password)
                    .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
            }

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}