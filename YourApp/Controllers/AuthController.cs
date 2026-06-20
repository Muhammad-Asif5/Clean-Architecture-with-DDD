using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourApp.Application.Common.Models;
using YourApp.Application.Users.Commands.ConfirmEmail;
using YourApp.Application.Users.Commands.Login;
using YourApp.Application.Users.Commands.RefreshToken;
using YourApp.Application.Users.Commands.Register;
using YourApp.Application.Users.Commands.RevokeToken;
using YourApp.Application.Users.Commands.UpdateProfile;
using YourApp.Application.Users.Queries.GetCurrentUser;

namespace YourApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerExtensions
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            if (command == null)
            {
                return BadRequest(ApiResponse<object>.BadRequestResponse(
                    ResponseType.BadRequest,
                    "Invalid request"
                ));
            }

            if (command.Password != command.ConfirmPassword)
            {
                return BadRequest(ApiResponse<object>.BadRequestResponse(
                    ResponseType.BadRequest,
                    "Passwords do not match",
                    new[] { "Password and confirmation password do not match" }
                ));
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.BadRequestResponse(
                    ResponseType.BadRequest,
                    "Registration failed",
                    result.Errors
                ));
            }

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<object>.CreatedResponse(result, ResponseType.Created, "User registered successfully"));
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse<object>.BadRequestResponse(
                    "ValidationError",
                    "User ID and token are required"
                ));
            }

            var result = await _mediator.Send(new ConfirmEmailCommand { UserId = userId, Token = token });

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.BadRequestResponse(
                    "EmailConfirmationFailed",
                    "Email confirmation failed",
                    result.Errors
                ));
            }

            // ✅ Now user can login - tokens are generated
            return Ok(ApiResponse<object>.SuccessResponse(
                result,
                "EmailConfirmed",
                "Your email has been confirmed successfully. You can now login."
            ));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            if (command == null)
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse());
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse(
                    "Unauthorized",
                    result.Errors[0]
                ));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Login", "Login successful"));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse(
                    "Unauthorized",
                    "Refresh token is required"
                ));
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse(
                    "Unauthorized",
                    "Invalid or expired refresh token"
                ));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "RefreshToken", "Token refreshed successfully"));
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command = null)
        {
            var refreshToken = command?.RefreshToken ??
                              Request.Cookies["RefreshToken"] ??
                              Request.Headers["X-Refresh-Token"].FirstOrDefault();

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _mediator.Send(new RevokeTokenCommand { RefreshToken = refreshToken });
            }

            Response.Cookies.Delete("RefreshToken");
            Response.Cookies.Delete("AccessToken");

            _logger.LogInformation("User {UserId} logged out", GetCurrentUserId());

            return StatusCode(StatusCodes.Status204NoContent,
                ApiResponse<object>.NoContentResponse("Logout", "Logged out successfully"));
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Me()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse());
            }

            var result = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });

            if (result == null)
            {
                return NotFound(ApiResponse<object>.NotFoundResponse("User not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Profile", "User profile retrieved successfully"));
        }

        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Profile()
        {
            var profile = GetProfile();

            if (profile == null)
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse());
            }

            _logger.LogInformation("User {UserId} profile accessed", profile.UserId);

            return Ok(ApiResponse<object>.SuccessResponse(profile, "Profile", "User profile retrieved successfully"));
        }

        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse());
            }

            command.UserId = userId;
            var result = await _mediator.Send(command);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.NotFoundResponse("User not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "UpdateProfile", "Profile updated successfully"));
        }

        [HttpGet("permissions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public IActionResult GetPermissions()
        {
            var profile = GetProfile();

            if (profile == null)
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse());
            }

            var permissions = new
            {
                profile.UserId,
                profile.UserName,
                profile.Email,
                profile.CanCreate,
                profile.CanRead,
                profile.CanUpdate,
                profile.CanDelete,
                profile.CanExport,
                Roles = profile.Roles.Select(r => r.Value)
            };

            return Ok(ApiResponse<object>.SuccessResponse(permissions, "Permissions", "User permissions retrieved successfully"));
        }
    }
}