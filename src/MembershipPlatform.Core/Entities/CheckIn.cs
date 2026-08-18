namespace MembershipPlatform.Core.Entities;

public sealed class CheckIn
{
    public Guid CheckInId { get; init; }

    public Guid MemberId { get; init; }

    public DateTimeOffset CheckInDate { get; init; }
}
