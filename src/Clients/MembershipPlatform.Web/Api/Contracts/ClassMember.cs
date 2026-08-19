namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a member registered for a class.
/// Used when displaying class registration details.
/// </summary>
public sealed record ClassMember(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);
