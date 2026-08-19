namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a member summary.
/// This is deliberately NOT shared with the backend to maintain boundary isolation.
/// The API uses its own contracts, and this client translates them.
/// </summary>
public sealed record MemberSummary(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);
