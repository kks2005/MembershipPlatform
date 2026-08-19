using System.Net;
using MembershipPlatform.Web.Api.Contracts;

namespace MembershipPlatform.Web.Api;

/// <summary>
/// Represents the result of an API operation.
/// This wrapper provides consistent handling of success and failure cases.
/// Includes the HTTP status code for detailed error handling.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public sealed record ApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    HttpStatusCode? StatusCode);

/// <summary>
/// Static factory methods for creating ApiResult instances.
/// Provides a clean API for constructing success and failure results.
/// </summary>
public static class ApiResult
{
    /// <summary>
    /// Creates a successful result with a value and status code.
    /// </summary>
    public static ApiResult<T> Success<T>(T value, HttpStatusCode statusCode) =>
        new(true, value, null, statusCode);

    /// <summary>
    /// Creates a failed result with an error and optional status code.
    /// </summary>
    public static ApiResult<T> Failure<T>(ApiError error, HttpStatusCode? statusCode = null) =>
        new(false, default, error, statusCode);
}
