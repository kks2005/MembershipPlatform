namespace MembershipPlatform.Application.Classes;

public sealed record RegisterMemberForClassResult(
    bool IsSuccess,
    Guid? RegistrationId,
    DateTimeOffset? RegisteredDate,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static RegisterMemberForClassResult Success(
        Guid registrationId,
        DateTimeOffset registeredDate) =>
        new(true, registrationId, registeredDate, null, null);

    public static RegisterMemberForClassResult Failure(string errorCode, string errorMessage) =>
        new(false, null, null, errorCode, errorMessage);
}
