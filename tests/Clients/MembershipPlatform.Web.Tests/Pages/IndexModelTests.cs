using System.Net;
using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using MembershipPlatform.Web.Pages;
using Moq;

namespace MembershipPlatform.Web.Tests.Pages;

[TestClass]
public sealed class IndexModelTests
{
    [TestMethod]
    public async Task GivenSuccessfulApiCallsWhenLoadingDashboardThenSetsAllProperties()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var members = new MemberSummary[]
        {
            new(Guid.NewGuid(), "Active Member", "active@example.com", "Active", DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Inactive Member", "inactive@example.com", "Inactive", DateTimeOffset.UtcNow)
        };
        var classes = new FitnessClass[]
        {
            new(Guid.NewGuid(), "Yoga", DateTimeOffset.UtcNow.AddDays(1), 20),
            new(Guid.NewGuid(), "Pilates", DateTimeOffset.UtcNow.AddDays(2), 15)
        };
        var summaries = new ClassRegistrationSummary[]
        {
            new(classes[0].ClassId, "Yoga", 20, 5),
            new(classes[1].ClassId, "Pilates", 15, 3)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(members, HttpStatusCode.OK));
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
        Assert.AreEqual(2, model.MemberCount);
        Assert.AreEqual(1, model.ActiveMemberCount);
        Assert.AreEqual(1, model.InactiveMemberCount);
        Assert.AreEqual(2, model.ClassCount);
        Assert.AreEqual(8, model.RegistrationCount);
        Assert.HasCount(2, model.UpcomingClasses);
        Assert.IsNull(model.Error);
        apiClient.Verify(client => client.GetMembersAsync(cancellationToken), Times.Once);
        apiClient.Verify(client => client.GetClassesAsync(cancellationToken), Times.Once);
        apiClient.Verify(client => client.GetRegistrationSummaryAsync(cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task GivenApiErrorWhenLoadingDashboardThenSetsErrorProperty()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var error = new ApiError("Client.Transport", "API is unavailable.", null);
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Failure<MemberSummary[]>(error));
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<FitnessClass>(), HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<ClassRegistrationSummary>(), HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(model.Error);
        Assert.AreEqual("Client.Transport", model.Error.Code);
        Assert.AreEqual("API is unavailable.", model.Error.Message);
    }

    [TestMethod]
    public async Task GivenClassIdExistsWhenGettingRegistrationCountThenReturnsCorrectCount()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var classes = new FitnessClass[]
        {
            new(classId, "Yoga", DateTimeOffset.UtcNow.AddDays(1), 20)
        };
        var summaries = new ClassRegistrationSummary[]
        {
            new(classId, "Yoga", 20, 5)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<MemberSummary>(), HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(classes, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(summaries, HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);
        await model.OnGetAsync(cancellationToken);

        // Act
        var count = model.GetRegistrationCount(classId);

        // Assert
        Assert.AreEqual(5, count);
    }

    [TestMethod]
    public async Task GivenClassIdNotFoundWhenGettingRegistrationCountThenReturnsZero()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<MemberSummary>(), HttpStatusCode.OK));
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
}
