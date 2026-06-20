using Microsoft.AspNetCore.Http;

namespace YourApp.Application.Common.Models
{
    // Response Type Constants
    public static class ResponseType
    {
        public const string Success = "Success";
        public const string RequestSuccessful = "Request processed successfully";
        public const string Created = "Created";
        public const string ResourceCreated = "Resource created successfully";
        public const string NoContent = "No content available";
        public const string BadRequest = "BadRequest";
        public const string ValidationErrors = "Validation errors occurred";
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string NotFound = "Not Found";
        public const string ServerError = "Internal Server Error";
        public const string DatabaseError = "Database Error";
        public const string Conflict = "Conflict";
        public const string ServiceUnavailable = "Service Unavailable";
    }

    // SpAction Enum
    public enum SpAction
    {
        Success,
        Created,
        NoContent,
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        ServerError,
        DatabaseError,
        ServiceUnavailable
    }
    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
    }

    public class ErrorResponse
    {
        public string Host { get; set; }
        public string Agent { get; set; }
        public string InnerMessage { get; set; }
        public string InnerException { get; set; }
        public string RequestParams { get; set; }
        public string Environment { get; set; }
        public string StackTrace { get; set; }
    }

    public class ActivityModel
    {
        public string UserId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ClientRequest { get; set; }
        public string ClientResponse { get; set; }
        public string RequestUrl { get; set; }
        public int StatusCode { get; set; }
        public string UserIp { get; set; }
        public string ErrorDetails { get; set; }

        public ActivityModel()
        {
        }

        public ActivityModel(
            string userId,
            DateTime activityDate,
            string clientRequest,
            string clientResponse,
            string requestUrl,
            int statusCode,
            string userIp,
            string errorDetails)
        {
            UserId = userId;
            ActivityDate = activityDate;
            ClientRequest = clientRequest;
            ClientResponse = clientResponse;
            RequestUrl = requestUrl;
            StatusCode = statusCode;
            UserIp = userIp;
            ErrorDetails = errorDetails;
        }
    }

    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();

        // 200 OK - Success Response with Data
        public static ApiResponse<T> SuccessResponse(T data, string action = ResponseType.Success, string message = ResponseType.RequestSuccessful)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Data = data,
                Action = action,
                Message = message
            };
        }

        public static async Task<ApiResponse<T>> SuccessResponseAsync(T data, string action = ResponseType.Success, string message = ResponseType.RequestSuccessful)
        {
            return await Task.FromResult(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Data = data,
                Action = action,
                Message = message
            });
        }

        // 200 OK - Success Response for List Data
        public static ApiResponse<List<T>> SuccessResponse(List<T> data, string action = ResponseType.Success, string message = ResponseType.RequestSuccessful)
        {
            return new ApiResponse<List<T>>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Data = data,
                Action = action,
                Message = message
            };
        }

        // 201 Created - When a new resource is successfully created
        public static ApiResponse<T> CreatedResponse(string action = ResponseType.Created, string message = ResponseType.ResourceCreated)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status201Created,
                Success = true,
                Action = action,
                Message = message
            };
        }

        // 201 Created - When a new resource is successfully created with data
        public static ApiResponse<T> CreatedResponse(T data, string action = ResponseType.Created, string message = ResponseType.ResourceCreated)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status201Created,
                Success = true,
                Data = data,
                Action = action,
                Message = message
            };
        }

        // 204 No Content - When the request is valid, but no data is available
        public static ApiResponse<T> NoContentResponse(string action = "NoContent", string message = ResponseType.NoContent)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status204NoContent,
                Success = true,
                Action = action,
                Message = message
            };
        }

        // 400 Bad Request - Invalid or missing parameters
        public static ApiResponse<T> BadRequestResponse(string action = ResponseType.BadRequest, string message = ResponseType.ValidationErrors, IEnumerable<string> errors = null)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Action = action,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        // 401 Unauthorized - When authentication fails
        public static ApiResponse<T> UnauthorizedResponse(string action = "Unauthorized", string message = ResponseType.Unauthorized)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Action = action,
                Message = message,
                Errors = new List<string> { "Unauthorized" }
            };
        }

        // 403 Access is forbidden
        public static ApiResponse<T> ForbiddenResponse(string action = "Forbidden", string message = ResponseType.Forbidden, IEnumerable<string> errors = null)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Success = false,
                Action = action,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        // 404 Not Found - When data is not found
        public static ApiResponse<T> NotFoundResponse(string action = "Not Found", string message = ResponseType.NotFound)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Action = action,
                Message = message
            };
        }

        // 404 Not Found - With custom message
        public static ApiResponse<object> NotFoundResponse(string message)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { "Not Found" },
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        // 409 Conflict - Resource conflict
        public static ApiResponse<T> ConflictResponse(string detail = "Resource conflict detected")
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Success = false,
                Action = SpAction.Conflict.ToString(),
                Message = detail
            };
        }

        public static ApiResponse<T> ConflictResponse(string action = "Conflict", string message = ResponseType.Conflict)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Success = false,
                Action = action,
                Message = message,
                Errors = new List<string> { "Conflict" }
            };
        }

        // 500 Internal Server Error - Generic Server Error
        public static ApiResponse<T> ServerErrorResponse(string action = "Error", string message = ResponseType.ServerError, IEnumerable<string> errors = null)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Success = false,
                Action = action,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        // 503 Service Unavailable - Database error or service unavailable
        public static ApiResponse<T> DatabaseErrorResponse(string action = ResponseType.DatabaseError, string message = ResponseType.DatabaseError, IEnumerable<string> errors = null)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Success = false,
                Action = action,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> ServiceUnavailable(string message = "Service temporarily unavailable")
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Success = false,
                Action = ResponseType.ServiceUnavailable,
                Message = message
            };
        }

        public static ApiResponse<T> ServiceUnavailable(SpAction action = SpAction.ServiceUnavailable, string message = ResponseType.ServiceUnavailable)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Success = false,
                Action = ResponseType.ServiceUnavailable,
                Message = message
            };
        }

        // Match pattern for functional programming
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ApiResponse<T>, TResult> onFailure)
            => Success ? onSuccess(Data) : onFailure(this);
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult()
        {
            Items = new List<T>();
        }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

}