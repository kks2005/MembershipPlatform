namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a newly created class registration.
/// Returned by the POST /registrations endpoint.
/// </summary>
public sealed record ClassRegistrationCreated(
    Guid RegistrationId,
    DateTimeOffset RegisteredDate);
