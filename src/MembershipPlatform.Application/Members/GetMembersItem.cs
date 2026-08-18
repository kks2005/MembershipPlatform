using MembershipPlatform.Core.Enums;

namespace MembershipPlatform.Application.Members;

public sealed record GetMembersItem(
    Guid MemberId,
    string Name,
    string Email,
    MemberStatus Status,
    DateTimeOffset JoinDate);
