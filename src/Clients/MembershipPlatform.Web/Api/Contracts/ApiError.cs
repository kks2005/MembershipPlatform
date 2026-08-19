namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Represents an error returned by the API.
/// Provides structured error information with a stable code, human-readable message,
/// and optional operation ID for correlation with server logs.
/// </summary>
/// <param name="Code">Stable error code (e.g., "Member.Inactive", "Class.AtCapacity")</param>
/// <param name="Message">Human-readable error description</param>
/// <param name="OperationId">Optional identifier for correlating with server-side logs</param>
public sealed record ApiError(
    string Code,
    string Message,
    string? OperationId);
