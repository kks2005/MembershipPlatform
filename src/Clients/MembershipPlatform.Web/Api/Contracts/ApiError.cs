namespace MembershipPlatform.Web.Api.Contracts;

public sealed record ApiError(
    string Code,
    string Message,
    string? OperationId);
