namespace MembershipPlatform.Core.Entities;

public sealed class FitnessClass
{
    public Guid ClassId { get; init; }

    public required string Name { get; init; }

    public DateTimeOffset StartTime { get; init; }

    public int Capacity { get; init; }

    public IReadOnlyCollection<ClassRegistration> ClassRegistrations { get; init; } = [];
}
