namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing registration statistics for a class.
/// Used to display capacity and remaining spots.
/// </summary>
public sealed record ClassRegistrationSummary(
    Guid ClassId,
    string ClassName,
    int Capacity,
    int RegistrationCount);
