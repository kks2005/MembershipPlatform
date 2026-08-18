namespace MembershipPlatform.Web.Api.Contracts;

public sealed record CheckInCreated(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
