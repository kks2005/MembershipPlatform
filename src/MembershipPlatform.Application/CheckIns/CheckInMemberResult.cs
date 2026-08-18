namespace MembershipPlatform.Application.CheckIns;

public sealed record CheckInMemberResult(
    bool IsSuccess,
    Guid? CheckInId,
    DateTimeOffset? CheckInDate,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static CheckInMemberResult Success(Guid checkInId, DateTimeOffset checkInDate) =>
        new(true, checkInId, checkInDate, null, null);

    public static CheckInMemberResult Failure(string errorCode, string errorMessage) =>
        new(false, null, null, errorCode, errorMessage);
}
