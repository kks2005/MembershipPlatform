using System.Net.Http.Json;
using System.Text.Json;

namespace MembershipPlatform.Blazor.Api;

public sealed class MembershipApiClient(HttpClient httpClient) : IMembershipApiClient
{
    public Task<ApiResult<MemberSummary[]>> GetMembersAsync(
        CancellationToken cancellationToken) =>
        GetAsync<MemberSummary[]>("api/v1/members", cancellationToken);

    public Task<ApiResult<FitnessClass[]>> GetClassesAsync(
        CancellationToken cancellationToken) =>
        GetAsync<FitnessClass[]>("api/v1/classes", cancellationToken);

    public Task<ApiResult<ClassRegistrationSummary[]>> GetRegistrationSummaryAsync(
        CancellationToken cancellationToken) =>
        GetAsync<ClassRegistrationSummary[]>(
            "api/v1/classes/registration-summary",
            cancellationToken);

    public Task<ApiResult<MemberCheckIn[]>> GetMemberCheckInsAsync(
        Guid memberId,
        CancellationToken cancellationToken) =>
        GetAsync<MemberCheckIn[]>(
            $"api/v1/members/{memberId:D}/check-ins",
            cancellationToken);

    public Task<ApiResult<FitnessClass[]>> GetMemberClassesAsync(
        Guid memberId,
        CancellationToken cancellationToken) =>
        GetAsync<FitnessClass[]>(
            $"api/v1/members/{memberId:D}/classes",
            cancellationToken);

    public Task<ApiResult<ClassMember[]>> GetClassMembersAsync(
        Guid classId,
        CancellationToken cancellationToken) =>
        GetAsync<ClassMember[]>(
            $"api/v1/classes/{classId:D}/members",
            cancellationToken);

    public Task<ApiResult<CheckInCreated>> CheckInMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken) =>
        PostAsync<CheckInCreated>(
            $"api/v1/members/{memberId:D}/check-ins",
            content: null,
            cancellationToken);

    public Task<ApiResult<ClassRegistrationCreated>> RegisterMemberAsync(
        Guid classId,
        Guid memberId,
        CancellationToken cancellationToken) =>
        PostAsync<ClassRegistrationCreated>(
            $"api/v1/classes/{classId:D}/registrations/{memberId:D}",
            content: null,
            cancellationToken);

    public async Task<ApiResult<MemberDocumentUploaded>> UploadMemberDocumentAsync(
        Guid memberId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken)
    {
        using var multipartContent = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new(contentType);
        multipartContent.Add(fileContent, "file", fileName);

        return await PostAsync<MemberDocumentUploaded>(
            $"api/v1/members/{memberId:D}/documents",
            multipartContent,
            cancellationToken);
    }

    private async Task<ApiResult<T>> GetAsync<T>(
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await SendAsync<T>(request, cancellationToken);
    }

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

    private async Task<ApiResult<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                return value is null
                    ? ApiResult.Failure<T>(new ApiError(
                        "Client.InvalidResponse",
                        "The API returned an empty response.",
                        null),
                        response.StatusCode)
                    : ApiResult.Success(value, response.StatusCode);
            }

            var error = await response.Content.ReadFromJsonAsync<ApiError>(cancellationToken)
                ?? new ApiError(
                    "Client.InvalidResponse",
                    "The API returned an unreadable error response.",
                    null);
            return ApiResult.Failure<T>(error, response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return TransportFailure<T>();
        }
        catch (NotSupportedException)
        {
            return InvalidResponse<T>();
        }
        catch (JsonException)
        {
            return InvalidResponse<T>();
        }
    }

    private static ApiResult<T> TransportFailure<T>() =>
        ApiResult.Failure<T>(new ApiError(
            "Client.Transport",
            "The API is unavailable or blocked by CORS.",
            null));

    private static ApiResult<T> InvalidResponse<T>() =>
        ApiResult.Failure<T>(new ApiError(
            "Client.InvalidResponse",
            "The API returned an invalid response.",
            null));
}
