namespace KomunitasKampus.API.Models;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    IReadOnlyDictionary<string, string[]>? Errors = null
)
{
    public static ApiResponse<T> Ok(
        T data,
        string message = "Request berhasil."
    )
    {
        return new ApiResponse<T>(
            Success: true,
            Message: message,
            Data: data
        );
    }

    public static ApiResponse<T> Fail(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null
    )
    {
        return new ApiResponse<T>(
            Success: false,
            Message: message,
            Data: default,
            Errors: errors
        );
    }
}