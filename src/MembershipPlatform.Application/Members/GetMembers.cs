using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Application.Members;

public sealed class GetMembers(IMemberRepository memberRepository)
{
    public async Task<IReadOnlyList<GetMembersItem>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var members = await memberRepository.GetAllAsync(cancellationToken);

        return members
            .Select(member => new GetMembersItem(
                member.MemberId,
                member.Name,
                member.Email,
                member.Status,
                member.JoinDate))
            .ToArray();
    }
}
