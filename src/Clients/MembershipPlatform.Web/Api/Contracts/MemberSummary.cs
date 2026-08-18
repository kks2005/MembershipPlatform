namespace MembershipPlatform.Web.Api.Contracts;

public sealed record MemberSummary(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);
