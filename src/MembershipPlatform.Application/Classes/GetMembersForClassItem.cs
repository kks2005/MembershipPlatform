using MembershipPlatform.Core.Enums;

namespace MembershipPlatform.Application.Classes;

public sealed record GetMembersForClassItem(
    Guid MemberId,
    string Name,
    string Email,
    MemberStatus Status,
    DateTimeOffset JoinDate);
