namespace MembershipPlatform.Web.Api.Contracts;

public sealed record ClassRegistrationCreated(
    Guid RegistrationId,
    DateTimeOffset RegisteredDate);
