namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a member check-in record.
/// </summary>
public sealed record MemberCheckIn(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
