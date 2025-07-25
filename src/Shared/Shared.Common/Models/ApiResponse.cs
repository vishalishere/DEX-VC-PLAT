// © 2024 DecVCPlat. All rights reserved.

namespace Shared.Common.Models;

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse SuccessResponse(string message = "Operation successful")
    {
        return new ApiResponse
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static ApiResponse ErrorResponse(string error)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    }

    public static ApiResponse ErrorResponse(List<string> errors)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static new ApiResponse<T> ErrorResponse(string error)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    }

    public static new ApiResponse<T> ErrorResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}
