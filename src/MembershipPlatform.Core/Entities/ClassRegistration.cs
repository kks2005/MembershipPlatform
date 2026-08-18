using MembershipPlatform.Core.Enums;

namespace MembershipPlatform.Core.Entities;

public sealed class ClassRegistration
{
    public Guid RegistrationId { get; init; }

    public Guid ClassId { get; init; }

    public Guid MemberId { get; init; }

    public DateTimeOffset RegisteredDate { get; init; }

    public RegistrationStatus Status { get; init; }
}
