namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a newly created check-in.
/// Returned by the POST /check-ins endpoint.
/// </summary>
public sealed record CheckInCreated(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
