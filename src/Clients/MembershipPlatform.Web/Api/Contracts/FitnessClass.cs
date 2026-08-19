namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a fitness class.
/// Maintained separately from backend domain entities to preserve client independence.
/// </summary>
public sealed record FitnessClass(
    Guid ClassId,
    string Name,
    DateTimeOffset StartTime,
    int Capacity);
