namespace MembershipPlatform.Api.Contracts.Members;

public sealed record MemberCheckInResponse(
    Guid CheckInId,
    DateTimeOffset CheckInDate);
