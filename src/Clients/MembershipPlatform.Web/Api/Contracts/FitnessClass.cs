namespace MembershipPlatform.Web.Api.Contracts;

public sealed record FitnessClass(
    Guid ClassId,
    string Name,
    DateTimeOffset StartTime,
    int Capacity);
