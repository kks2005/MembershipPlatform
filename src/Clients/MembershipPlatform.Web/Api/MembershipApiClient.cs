using System.Net.Http.Json;
using MembershipPlatform.Web.Api.Contracts;

namespace MembershipPlatform.Web.Api;

/// <summary>
/// Client-owned HTTP client that implements the IMembershipApiClient interface.
/// This client communicates with the versioned HTTP API without sharing domain entities.
/// Uses client-specific contracts to maintain boundary isolation.
/// </summary>
public sealed class MembershipApiClient(HttpClient httpClient) : IMembershipApiClient
{
    // ==================== GET Operations ====================
    // These methods retrieve data from the API without modifying state

    /// <summary>
    /// Retrieves all members from the API.
    /// Demonstrates: Simple GET request to a collection endpoint.
    /// </summary>
    public Task<ApiResult<MemberSummary[]>> GetMembersAsync(
        CancellationToken cancellationToken) =>
        GetAsync<MemberSummary[]>("api/v1/members", cancellationToken);

    /// <summary>
    /// Retrieves all fitness classes from the API.
    /// Demonstrates: Simple GET request to a collection endpoint.
    /// </summary>
    public Task<ApiResult<FitnessClass[]>> GetClassesAsync(
        CancellationToken cancellationToken) =>
        GetAsync<FitnessClass[]>("api/v1/classes", cancellationToken);

    /// <summary>
    /// Retrieves registration summary for all classes.
    /// Demonstrates: Aggregated read-only query endpoint.
    /// </summary>
    public Task<ApiResult<ClassRegistrationSummary[]>> GetRegistrationSummaryAsync(
        CancellationToken cancellationToken) =>
        GetAsync<ClassRegistrationSummary[]>(
            "api/v1/classes/registration-summary",
            cancellationToken);

    /// <summary>
    /// Retrieves check-in history for a specific member.
    /// Demonstrates: Parameterized GET request with member ID in the URL.
    /// </summary>
    public Task<ApiResult<MemberCheckIn[]>> GetMemberCheckInsAsync(
        Guid memberId,
        CancellationToken cancellationToken) =>
        GetAsync<MemberCheckIn[]>(
            $"api/v1/members/{memberId:D}/check-ins",
            cancellationToken);

    /// <summary>
    /// Retrieves classes registered by a specific member.
    /// Demonstrates: Relationship query (member-to-classes).
    /// </summary>
    public Task<ApiResult<FitnessClass[]>> GetMemberClassesAsync(
        Guid memberId,
        CancellationToken cancellationToken) =>
        GetAsync<FitnessClass[]>(
            $"api/v1/members/{memberId:D}/classes",
            cancellationToken);

    /// <summary>
    /// Retrieves members registered for a specific class.
    /// Demonstrates: Relationship query (class-to-members).
    /// </summary>
    public Task<ApiResult<ClassMember[]>> GetClassMembersAsync(
        Guid classId,
        CancellationToken cancellationToken) =>
        GetAsync<ClassMember[]>(
            $"api/v1/classes/{classId:D}/members",
            cancellationToken);

    // ==================== POST Operations ====================
    // These methods create new records or trigger actions

    /// <summary>
    /// Creates a check-in for a member.
    /// Demonstrates: POST to create a resource. Returns the created check-in details.
    /// Business rules: Only active members can check in.
    /// </summary>
    public Task<ApiResult<CheckInCreated>> CheckInMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken) =>
        PostAsync<CheckInCreated>(
            $"api/v1/members/{memberId:D}/check-ins",
            content: null,
            cancellationToken);

    /// <summary>
    /// Registers a member for a fitness class.
    /// Demonstrates: POST to create a relationship. Returns the registration details.
    /// Business rules: 
    /// - Only active members can register
    /// - Class must have available capacity
    /// - Member cannot be registered twice
    /// </summary>
    public Task<ApiResult<ClassRegistrationCreated>> RegisterMemberAsync(
        Guid classId,
        Guid memberId,
        CancellationToken cancellationToken) =>
        PostAsync<ClassRegistrationCreated>(
            $"api/v1/classes/{classId:D}/registrations/{memberId:D}",
            content: null,
            cancellationToken);

    /// <summary>
    /// Uploads a document for a member.
    /// Demonstrates: Multipart form-data upload with file content.
    /// Returns an opaque storage key (infrastructure-independent).
    /// </summary>
    public async Task<ApiResult<MemberDocumentUploaded>> UploadMemberDocumentAsync(
        Guid memberId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken)
    {
        // Create multipart content for file upload
        using var multipartContent = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new(contentType);
        multipartContent.Add(fileContent, "file", fileName);

        return await PostAsync<MemberDocumentUploaded>(
            $"api/v1/members/{memberId:D}/documents",
            multipartContent,
            cancellationToken);
    }

    // ==================== Private Helper Methods ====================
    // These methods handle HTTP communication details

    /// <summary>
    /// Executes a GET request and deserializes the JSON response.
    /// </summary>
    private async Task<ApiResult<T>> GetAsync<T>(
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Executes a POST request with optional content and deserializes the JSON response.
    /// </summary>
    private async Task<ApiResult<T>> PostAsync<T>(
        string requestUri,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };
        return await SendAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Core HTTP communication method that:
    /// 1. Sends the HTTP request
    /// 2. Checks the response status
    /// 3. Deserializes success or error responses
    /// 4. Handles transport-level exceptions
    /// Returns a structured ApiResult with success/failure information.
    /// </summary>
    private async Task<ApiResult<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Send the request to the API
            using var response = await httpClient.SendAsync(request, cancellationToken);

            // Check if the request was successful (2xx status code)
            if (response.IsSuccessStatusCode)
            {
                // Deserialize the JSON response body
                var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken);

                return value is null
                    ? ApiResult.Failure<T>(new ApiError(
                        "Client.InvalidResponse",
                        "The API returned an empty response.",
                        null),
                        response.StatusCode)
                    : ApiResult.Success(value, response.StatusCode);
            }

            // Handle error responses (4xx, 5xx status codes)
            // The API returns structured error information as JSON
            var error = await response.Content.ReadFromJsonAsync<ApiError>(cancellationToken)
                ?? new ApiError(
                    "Client.InvalidResponse",
                    "The API returned an unreadable error response.",
                    null);
            return ApiResult.Failure<T>(error, response.StatusCode);
        }
        catch (HttpRequestException)
        {
            // Handle network-level failures (API unavailable, connection refused, etc.)
            return ApiResult.Failure<T>(new ApiError(
                "Client.Transport",
                "The API is unavailable. Start the API and try again.",
                null));
        }
        catch (NotSupportedException)
        {
            // Handle unsupported media types or content
            return ApiResult.Failure<T>(new ApiError(
                "Client.InvalidResponse",
                "The API returned an unsupported response.",
                null));
        }
        catch (System.Text.Json.JsonException)
        {
            // Handle JSON deserialization failures
            return ApiResult.Failure<T>(new ApiError(
                "Client.InvalidResponse",
                "The API returned invalid JSON.",
                null));
        }
    }
}
