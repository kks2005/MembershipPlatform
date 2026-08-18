namespace MembershipPlatform.Core.Storage;

public interface IMemberDocumentStorage
{
    Task<MemberDocumentReference> SaveAsync(
        Guid memberId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default);
}
