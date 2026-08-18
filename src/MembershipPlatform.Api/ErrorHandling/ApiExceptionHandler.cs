using MembershipPlatform.Api.Contracts.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace MembershipPlatform.Api.ErrorHandling;

public sealed partial class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var operationId = httpContext.TraceIdentifier;

        LogUnexpectedApiFailure(
            logger,
            exception,
            operationId,
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(
                ApiErrorCodes.SystemUnexpected,
                "An unexpected error occurred.",
                operationId),
            cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unexpected API failure for operation {OperationId} on {RequestMethod} {RequestPath}")]
    private static partial void LogUnexpectedApiFailure(
        ILogger logger,
        Exception exception,
        string operationId,
        string requestMethod,
        string requestPath);
}
