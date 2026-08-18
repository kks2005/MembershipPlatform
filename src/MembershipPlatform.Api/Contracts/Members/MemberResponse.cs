namespace MembershipPlatform.Api.Contracts.Members;

public sealed record MemberResponse(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);
