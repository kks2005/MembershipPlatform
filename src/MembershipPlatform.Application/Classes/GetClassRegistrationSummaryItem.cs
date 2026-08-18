namespace MembershipPlatform.Application.Classes;

public sealed record GetClassRegistrationSummaryItem(
    Guid ClassId,
    string ClassName,
    int Capacity,
    int RegistrationCount);
