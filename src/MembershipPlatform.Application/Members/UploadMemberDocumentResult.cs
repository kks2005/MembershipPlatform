namespace MembershipPlatform.Application.Members;

public sealed record UploadMemberDocumentResult(
    bool IsSuccess,
    string? StorageKey,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static UploadMemberDocumentResult Success(string storageKey) =>
        new(true, storageKey, null, null);

    public static UploadMemberDocumentResult Failure(string errorCode, string errorMessage) =>
        new(false, null, errorCode, errorMessage);
}
