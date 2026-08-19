using System.Net;
using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using MembershipPlatform.Web.Pages.Classes;
using Moq;

namespace MembershipPlatform.Web.Tests.Pages.Classes;

[TestClass]
public sealed class IndexModelTests
{
    [TestMethod]
    public async Task GivenSuccessfulApiCallsWhenLoadingClassesThenSetsProperties()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var classes = new FitnessClass[]
        {
            new(classId, "Yoga", DateTimeOffset.UtcNow.AddDays(2), 20),
            new(Guid.NewGuid(), "Pilates", DateTimeOffset.UtcNow.AddDays(1), 15)
        };
        var summaries = new ClassRegistrationSummary[]
        {
            new(classId, "Yoga", 20, 5)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(classes, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(summaries, HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync(cancellationToken);

        // Assert
        Assert.HasCount(2, model.Classes);
        Assert.AreEqual("Pilates", model.Classes[0].Name);
        Assert.AreEqual(5, model.GetRegistrationCount(classId));
        Assert.IsNull(model.Error);
    }

    [TestMethod]
    public async Task GivenClassNotInSummaryWhenGettingRegistrationCountThenReturnsZero()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<FitnessClass>(), HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<ClassRegistrationSummary>(), HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);
        await model.OnGetAsync(cancellationToken);

        // Act
        var count = model.GetRegistrationCount(Guid.NewGuid());

        // Assert
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task GivenApiErrorWhenLoadingClassesThenSetsErrorProperty()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var error = new ApiError("Client.Transport", "API is unavailable.", null);
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Failure<FitnessClass[]>(error));
        apiClient
            .Setup(client => client.GetRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<ClassRegistrationSummary>(), HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(model.Error);
        Assert.AreEqual("Client.Transport", model.Error.Code);
    }
}
