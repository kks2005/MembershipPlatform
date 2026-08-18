using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Application.CheckIns;

public sealed class GetMemberCheckIns(ICheckInRepository checkInRepository)
{
    public async Task<IReadOnlyList<GetMemberCheckInsItem>> ExecuteAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var checkIns = await checkInRepository.GetByMemberIdAsync(
            memberId,
            cancellationToken);

        return checkIns
            .Select(checkIn => new GetMemberCheckInsItem(
                checkIn.CheckInId,
                checkIn.CheckInDate))
            .ToArray();
    }
}
