namespace MembershipPlatform.Api.Contracts.Classes;

public sealed record ClassRegistrationSummaryResponse(
    Guid ClassId,
    string ClassName,
    int Capacity,
    int RegistrationCount);
