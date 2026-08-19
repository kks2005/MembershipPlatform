namespace MembershipPlatform.Web.Api.Contracts;

/// <summary>
/// Client-owned contract representing a successfully uploaded document.
/// The StorageKey is an opaque identifier - the client doesn't know if it's
/// a file path, blob URL, S3 key, or any other storage mechanism.
/// This demonstrates infrastructure independence.
/// </summary>
public sealed record MemberDocumentUploaded(
    string StorageKey);
