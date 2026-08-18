using MembershipPlatform.Core.Enums;

namespace MembershipPlatform.Core.Entities;

public sealed class Member
{
    public Guid MemberId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public MemberStatus Status { get; init; }

    public DateTimeOffset JoinDate { get; init; }

    public IReadOnlyCollection<CheckIn> CheckIns { get; init; } = [];

    public IReadOnlyCollection<ClassRegistration> ClassRegistrations { get; init; } = [];
}
