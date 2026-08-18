namespace MembershipPlatform.Api.Contracts.Classes;

public sealed record ClassResponse(
    Guid ClassId,
    string Name,
    DateTimeOffset StartTime,
    int Capacity);
