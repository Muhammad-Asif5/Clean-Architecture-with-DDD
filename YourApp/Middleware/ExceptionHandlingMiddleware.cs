using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;
using System.Text;
using YourApp.Application.Common.Exceptions;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Common.Models;

namespace YourApp.API.Middleware
{
    public class GlobalExceptionMiddleware : IMiddleware
    {
        private readonly IActivityService _activityService;
        private readonly IHostEnvironment _env;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            IActivityService activityService,
            IHostEnvironment env,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _activityService = activityService;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var responseBodyStream = context.Response.Body;
            await using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await next(context);
                await CopyResponseToOriginalStream(context, memoryStream, responseBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex, responseBodyStream);
            }
        }

        /// <summary>
        /// Handles the exception, sets the response, and logs the error.
        /// </summary>
        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            Stream responseBodyStream)
        {
            context.Response.ContentType = "application/json";

            // Map exceptions to responses
            var (response, statusCode) = MapExceptionToResponse(exception);

            context.Response.StatusCode = statusCode;
            context.Response.Body = responseBodyStream;

            // Create detailed error response for logging
            var exceptionResponse = new ErrorResponse
            {
                Host = context.Request.Headers["Host"],
                Agent = context.Request.Headers["User-Agent"],
                InnerMessage = exception.Message,
                InnerException = GetExceptionDetails(exception),
                RequestParams = await GetRequestBodyAsync(context),
                Environment = _env.EnvironmentName,
                StackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            };

            // Create and save activity log
            var logEntry = await CreateActivityLog(context, exceptionResponse, statusCode, response);
            await SaveActivityLog(logEntry);

            _logger.LogError("Exception_Response: {@LogEntry}", logEntry);

            await context.Response.WriteAsJsonAsync(response);
        }

        private (object response, int statusCode) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                ValidationException vex => (
                    ApiResponse<object>.BadRequestResponse(
                        ResponseType.BadRequest,
                        ResponseType.ValidationErrors,
                        vex.Errors.Select(e => e.ErrorMessage)
                    ),
                    StatusCodes.Status400BadRequest
                ),
                UnauthorizedAccessException => (
                    ApiResponse<object>.UnauthorizedResponse("Authentication required"),
                    StatusCodes.Status401Unauthorized
                ),
                PermissionDeniedException => (
                    ApiResponse<object>.ForbiddenResponse("Insufficient permissions"),
                    StatusCodes.Status403Forbidden
                ),
                KeyNotFoundException or NotFoundException => (
                    ApiResponse<object>.NotFoundResponse(exception.Message),
                    StatusCodes.Status404NotFound
                ),
                ConflictException => (
                    ApiResponse<object>.ConflictResponse(exception.Message),
                    StatusCodes.Status409Conflict
                ),
                TimeoutException => (
                    ApiResponse<object>.ServiceUnavailable("Request timeout occurred"),
                    StatusCodes.Status408RequestTimeout
                ),
                DbUpdateException dbEx when dbEx.InnerException is SqlException sqlExInner => (
                    HandleSqlException(sqlExInner),
                    GetStatusCodeForSqlException(sqlExInner)
                ),
                DbUpdateException dbEx => (
                    HandleDbUpdateException(dbEx),
                    StatusCodes.Status409Conflict
                ),
                SqlException sqlEx => (
                    HandleSqlException(sqlEx),
                    GetStatusCodeForSqlException(sqlEx)
                ),
                DbException dbException => (
                    ApiResponse<object>.DatabaseErrorResponse(
                        ResponseType.DatabaseError,
                        "Database connection error",
                        new[] { dbException.Message }
                    ),
                    StatusCodes.Status503ServiceUnavailable
                ),
                HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Unauthorized => (
                    ApiResponse<object>.UnauthorizedResponse(
                        "Unauthorized",
                        "External service authentication failed"
                    ),
                    StatusCodes.Status401Unauthorized
                ),
                HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Forbidden => (
                    ApiResponse<object>.ForbiddenResponse(
                        "Forbidden",
                        "External service access denied",
                        new[] { httpEx.Message }
                    ),
                    StatusCodes.Status403Forbidden
                ),
                HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound => (
                    ApiResponse<object>.NotFoundResponse("External resource not found"),
                    StatusCodes.Status404NotFound
                ),
                HttpRequestException => (
                    ApiResponse<object>.ServerErrorResponse(
                        "Error",
                        "External service communication failed",
                        new[] { exception.Message }
                    ),
                    StatusCodes.Status502BadGateway
                ),
                _ => (
                    ApiResponse<object>.ServerErrorResponse(
                        "Error",
                        _env.IsDevelopment() ? exception.Message : "An internal server error occurred",
                        _env.IsDevelopment() ? new[] { exception.StackTrace } : null
                    ),
                    StatusCodes.Status500InternalServerError
                )
            };
        }

        private static ApiResponse<object> HandleSqlException(SqlException sqlEx)
        {
            var errorMessage = sqlEx.Message;

            // Handle custom database exceptions
            if (errorMessage.StartsWith("DB_Exception:"))
            {
                errorMessage = errorMessage.Split(':')[1].Trim();
                return ApiResponse<object>.BadRequestResponse(
                    "Database operation failed", errorMessage
                );
            }

            // Handle specific SQL error numbers
            return sqlEx.Number switch
            {
                // Constraint violations
                547 => ApiResponse<object>.ConflictResponse("Foreign key constraint violation"),
                2627 or 2601 => ApiResponse<object>.ConflictResponse("Duplicate key violation"),

                // Connection and availability issues
                18456 => ApiResponse<object>.UnauthorizedResponse("Database authentication failed"),
                4060 => ApiResponse<object>.ServiceUnavailable("Database unavailable"),
                53 or 121 => ApiResponse<object>.ServiceUnavailable("Database server not found"),
                1205 => ApiResponse<object>.ConflictResponse("Deadlock victim"),

                // Timeout issues
                -2 => ApiResponse<object>.ServiceUnavailable("Database connection timeout"),

                // Resource issues
                701 => ApiResponse<object>.ServiceUnavailable("Insufficient database memory"),
                8645 => ApiResponse<object>.ServiceUnavailable("Database resource limit reached"),

                _ => ApiResponse<object>.ServerErrorResponse("Database error occurred")
            };
        }

        private static ApiResponse<object> HandleDbUpdateException(DbUpdateException dbEx)
        {
            var message = dbEx.Message;

            // Handle unique constraint violations
            if (message.Contains("unique") || message.Contains("duplicate"))
            {
                return ApiResponse<object>.ConflictResponse("Duplicate record violation");
            }

            // Handle foreign key violations
            if (message.Contains("foreign") || message.Contains("constraint"))
            {
                return ApiResponse<object>.ConflictResponse("Reference constraint violation");
            }

            // Handle null constraints
            if (message.Contains("null") || message.Contains("required"))
            {
                return ApiResponse<object>.BadRequestResponse("Required field missing");
            }

            return ApiResponse<object>.ConflictResponse("Database update failed");
        }

        private static int GetStatusCodeForSqlException(SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                // Conflict status for constraint violations
                547 or 2627 or 2601 or 1205 => StatusCodes.Status409Conflict,

                // Unauthorized for authentication issues
                18456 => StatusCodes.Status401Unauthorized,

                // Service unavailable for connection/issues
                4060 or 53 or 121 or -2 or 701 or 8645 => StatusCodes.Status503ServiceUnavailable,

                // Default to internal server error
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private async Task<ActivityModel> CreateActivityLog(
            HttpContext context,
            ErrorResponse error,
            int statusCode,
            object apiResponse)
        {
            var userIp = GetClientIpAddress(context);
            var userId = context.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous"
                : "Anonymous";

            return new ActivityModel(
                userId: userId,
                activityDate: DateTime.UtcNow,
                clientRequest: await GetRequestPayload(context),
                clientResponse: JsonConvert.SerializeObject(apiResponse),
                requestUrl: $"{context.Request.Method} {context.Request.Path}",
                statusCode: statusCode,
                userIp: userIp,
                errorDetails: _env.IsProduction() ? null : JsonConvert.SerializeObject(error)
            );
        }

        private async Task SaveActivityLog(ActivityModel logEntry)
        {
            try
            {
                await _activityService.SaveActivityLog(logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save activity log for error");
            }
        }

        private static string GetExceptionDetails(Exception exception)
        {
            var traceString = new StringBuilder();
            var frames = new System.Diagnostics.StackTrace(exception, true).GetFrames();

            if (frames != null)
            {
                foreach (var frame in frames)
                {
                    if (frame.GetFileLineNumber() < 1) continue;
                    traceString.AppendLine($"File: {frame.GetFileName()}, Method: {frame.GetMethod()?.Name}, Line: {frame.GetFileLineNumber()}");
                }
            }

            return traceString.Length > 0 ? traceString.ToString() : exception.StackTrace ?? "No stack trace available";
        }

        private static async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            if (context.Request.Method != HttpMethods.Post && context.Request.Method != HttpMethods.Put)
                return JsonConvert.SerializeObject(context.Request.Query);

            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                return body.Length > 1000 ? body[..1000] + "... [truncated]" : body;
            }
            catch
            {
                return "Unable to read request body";
            }
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "Unknown";
        }

        private async Task<string> GetRequestPayload(HttpContext context)
        {
            if (context.Request.ContentLength > 0 && context.Request.Method != HttpMethods.Get)
            {
                try
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var payload = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                    return payload.Length > 2000 ? payload[..2000] + "... [truncated]" : payload;
                }
                catch (Exception ex)
                {
                    return $"Error reading payload: {ex.Message}";
                }
            }
            return string.Empty;
        }

        private static async Task CopyResponseToOriginalStream(
            HttpContext context,
            MemoryStream memoryStream,
            Stream originalStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalStream);
            context.Response.Body = originalStream;
        }
    }
}