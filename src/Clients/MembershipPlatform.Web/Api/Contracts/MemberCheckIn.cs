namespace MembershipPlatform.Web.Api.Contracts;

public sealed record MemberCheckIn(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
