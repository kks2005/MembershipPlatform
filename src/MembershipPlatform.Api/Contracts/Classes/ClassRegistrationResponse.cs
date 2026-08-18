namespace MembershipPlatform.Api.Contracts.Classes;

public sealed record ClassRegistrationResponse(
    Guid RegistrationId,
    DateTimeOffset RegisteredDate);
