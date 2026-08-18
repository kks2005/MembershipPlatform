using MembershipPlatform.Web.Api.Contracts;

namespace MembershipPlatform.Web.Api;

public interface IMembershipApiClient
{
    Task<ApiResult<MemberSummary[]>> GetMembersAsync(CancellationToken cancellationToken);

    Task<ApiResult<FitnessClass[]>> GetClassesAsync(CancellationToken cancellationToken);

    Task<ApiResult<ClassRegistrationSummary[]>> GetRegistrationSummaryAsync(
        CancellationToken cancellationToken);

    Task<ApiResult<MemberCheckIn[]>> GetMemberCheckInsAsync(
        Guid memberId,
        CancellationToken cancellationToken);

    Task<ApiResult<FitnessClass[]>> GetMemberClassesAsync(
        Guid memberId,
        CancellationToken cancellationToken);

    Task<ApiResult<ClassMember[]>> GetClassMembersAsync(
        Guid classId,
        CancellationToken cancellationToken);

    Task<ApiResult<CheckInCreated>> CheckInMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken);

    Task<ApiResult<ClassRegistrationCreated>> RegisterMemberAsync(
        Guid classId,
        Guid memberId,
        CancellationToken cancellationToken);

    Task<ApiResult<MemberDocumentUploaded>> UploadMemberDocumentAsync(
        Guid memberId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken);
}
