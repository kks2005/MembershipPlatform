using System.Net;

namespace MembershipPlatform.Blazor.Api;

public sealed record ApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    HttpStatusCode? StatusCode);

public static class ApiResult
{
    public static ApiResult<T> Success<T>(T value, HttpStatusCode statusCode) =>
        new(true, value, null, statusCode);

    public static ApiResult<T> Failure<T>(ApiError error, HttpStatusCode? statusCode = null) =>
        new(false, default, error, statusCode);
}
