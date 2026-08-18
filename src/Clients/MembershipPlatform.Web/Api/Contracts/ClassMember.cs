namespace MembershipPlatform.Web.Api.Contracts;

public sealed record ClassMember(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);
