using MembershipPlatform.Core.Queries;

namespace MembershipPlatform.Application.Classes;

public sealed class GetClassesForMember(IMemberClassQuery memberClassQuery)
{
    public async Task<IReadOnlyList<GetClassesItem>> ExecuteAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var classes = await memberClassQuery.GetClassesForMemberAsync(
            memberId,
            cancellationToken);

        return classes
            .Select(fitnessClass => new GetClassesItem(
                fitnessClass.ClassId,
                fitnessClass.Name,
                fitnessClass.StartTime,
                fitnessClass.Capacity))
            .ToArray();
    }
}
