namespace MembershipPlatform.Blazor.Api;

public sealed record ApiError(string Code, string Message, string? OperationId);

public sealed record MemberSummary(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);

public sealed record FitnessClass(
    Guid ClassId,
    string Name,
    DateTimeOffset StartTime,
    int Capacity);

public sealed record MemberCheckIn(Guid CheckInId, DateTimeOffset CheckInDate);

public sealed record ClassMember(
    Guid MemberId,
    string Name,
    string Email,
    string Status,
    DateTimeOffset JoinDate);

public sealed record ClassRegistrationSummary(
    Guid ClassId,
    string ClassName,
    int Capacity,
    int RegistrationCount);

public sealed record CheckInCreated(Guid CheckInId, DateTimeOffset CheckInDate);

public sealed record ClassRegistrationCreated(
    Guid RegistrationId,
    DateTimeOffset RegisteredDate);

public sealed record MemberDocumentUploaded(string StorageKey);
