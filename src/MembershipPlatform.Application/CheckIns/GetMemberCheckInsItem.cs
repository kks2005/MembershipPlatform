namespace MembershipPlatform.Application.CheckIns;

public sealed record GetMemberCheckInsItem(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
