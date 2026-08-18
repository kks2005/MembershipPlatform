namespace MembershipPlatform.Web.Api.Contracts;

public sealed record ClassRegistrationSummary(
    Guid ClassId,
    string ClassName,
    int Capacity,
    int RegistrationCount);
