namespace MembershipPlatform.Application.Classes;

public sealed record GetClassesItem(
    Guid ClassId,
    string Name,
    DateTimeOffset StartTime,
    int Capacity);
