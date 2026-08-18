namespace MembershipPlatform.Api.Contracts.Members;

public sealed record CheckInResponse(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
