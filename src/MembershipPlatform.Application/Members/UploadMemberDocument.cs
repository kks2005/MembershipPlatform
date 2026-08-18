using MembershipPlatform.Core.Repositories;
using MembershipPlatform.Core.Storage;

namespace MembershipPlatform.Application.Members;

public sealed class UploadMemberDocument(
    IMemberRepository memberRepository,
    IMemberDocumentStorage memberDocumentStorage)
{
    public async Task<UploadMemberDocumentResult> ExecuteAsync(
        Guid memberId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

        if (member is null)
        {
            return UploadMemberDocumentResult.Failure(
                ApplicationErrorCodes.MemberNotFound,
                "Member not found.");
        }

        var documentReference = await memberDocumentStorage.SaveAsync(
            memberId,
            fileName,
            contentType,
            content,
            cancellationToken);

        return UploadMemberDocumentResult.Success(documentReference.StorageKey);
    }
}
