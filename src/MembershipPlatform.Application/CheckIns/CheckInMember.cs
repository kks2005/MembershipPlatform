using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Application.CheckIns;

public sealed class CheckInMember(
    IMemberRepository memberRepository,
    ICheckInRepository checkInRepository)
{
    public async Task<CheckInMemberResult> ExecuteAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

        if (member is null)
        {
            return CheckInMemberResult.Failure(
                ApplicationErrorCodes.MemberNotFound,
                "Member not found.");
        }

        if (member.Status != MemberStatus.Active)
        {
            return CheckInMemberResult.Failure(
                ApplicationErrorCodes.MemberInactive,
                "Member is not active.");
        }

        var checkIn = new CheckIn
        {
            CheckInId = Guid.NewGuid(),
            MemberId = memberId,
            CheckInDate = DateTimeOffset.UtcNow
        };

        await checkInRepository.AddAsync(checkIn, cancellationToken);

        return CheckInMemberResult.Success(checkIn.CheckInId, checkIn.CheckInDate);
    }
}
