namespace MembershipPlatform.Api.Contracts.Classes;

public sealed record ClassMemberResponse(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);
