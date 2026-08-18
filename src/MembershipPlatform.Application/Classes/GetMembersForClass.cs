using MembershipPlatform.Core.Queries;

namespace MembershipPlatform.Application.Classes;

public sealed class GetMembersForClass(IClassRegistrationQuery classRegistrationQuery)
{
    public async Task<IReadOnlyList<GetMembersForClassItem>> ExecuteAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        var members = await classRegistrationQuery.GetMembersForClassAsync(
            classId,
            cancellationToken);

        return members
            .Select(member => new GetMembersForClassItem(
                member.MemberId,
                member.Name,
                member.Email,
                member.Status,
                member.JoinDate))
            .ToArray();
    }
}
