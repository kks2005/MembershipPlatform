using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Application.Classes;

public sealed class GetClasses(IClassRepository classRepository)
{
    public async Task<IReadOnlyList<GetClassesItem>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var classes = await classRepository.GetAllAsync(cancellationToken);

        return classes
            .Select(fitnessClass => new GetClassesItem(
                fitnessClass.ClassId,
                fitnessClass.Name,
                fitnessClass.StartTime,
                fitnessClass.Capacity))
            .ToArray();
    }
}
