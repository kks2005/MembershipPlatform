namespace MembershipPlatform.Core.Queries;

public sealed record ClassRegistrationSummary(
    Guid ClassId,
    string ClassName,
    int Capacity,
    int RegistrationCount);
