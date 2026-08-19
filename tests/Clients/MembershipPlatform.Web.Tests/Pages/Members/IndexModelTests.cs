using System.Net;
using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using MembershipPlatform.Web.Pages.Members;
using Moq;

namespace MembershipPlatform.Web.Tests.Pages.Members;

[TestClass]
public sealed class IndexModelTests
{
    [TestMethod]
    public async Task GivenNoStatusFilterWhenLoadingMembersThenShowsAllMembers()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var members = new MemberSummary[]
        {
            new(Guid.NewGuid(), "Active Member", "active@example.com", "Active", DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Inactive Member", "inactive@example.com", "Inactive", DateTimeOffset.UtcNow)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(members, HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync(null, cancellationToken);

        // Assert
        Assert.HasCount(2, model.Members);
        Assert.AreEqual("All", model.SelectedStatus);
        Assert.AreEqual(2, model.TotalCount);
        Assert.AreEqual(1, model.ActiveCount);
        Assert.AreEqual(1, model.InactiveCount);
        Assert.IsNull(model.Error);
    }

    [TestMethod]
    public async Task GivenActiveStatusFilterWhenLoadingMembersThenShowsOnlyActiveMembers()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var members = new MemberSummary[]
        {
            new(Guid.NewGuid(), "Active Member", "active@example.com", "Active", DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Inactive Member", "inactive@example.com", "Inactive", DateTimeOffset.UtcNow)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(members, HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync("Active", cancellationToken);

        // Assert
        Assert.HasCount(1, model.Members);
        Assert.AreEqual("Active", model.SelectedStatus);
        Assert.AreEqual("Active Member", model.Members[0].Name);
    }

    [TestMethod]
    public async Task GivenInactiveStatusFilterWhenLoadingMembersThenShowsOnlyInactiveMembers()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var members = new MemberSummary[]
        {
            new(Guid.NewGuid(), "Active Member", "active@example.com", "Active", DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Inactive Member", "inactive@example.com", "Inactive", DateTimeOffset.UtcNow)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(members, HttpStatusCode.OK));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync("Inactive", cancellationToken);

        // Assert
        Assert.HasCount(1, model.Members);
        Assert.AreEqual("Inactive", model.SelectedStatus);
        Assert.AreEqual("Inactive Member", model.Members[0].Name);
    }

    [TestMethod]
    public async Task GivenApiErrorWhenLoadingMembersThenSetsErrorProperty()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var error = new ApiError("Client.Transport", "API is unavailable.", null);
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Failure<MemberSummary[]>(error));
        var model = new IndexModel(apiClient.Object);

        // Act
        await model.OnGetAsync(null, cancellationToken);

        // Assert
        Assert.IsNotNull(model.Error);
        Assert.AreEqual("Client.Transport", model.Error.Code);
        Assert.IsEmpty(model.Members);
    }
}
