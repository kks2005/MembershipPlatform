using System.Net;
using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using MembershipPlatform.Web.Pages.Members;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace MembershipPlatform.Web.Tests.Pages.Members;

[TestClass]
public sealed class DetailsModelTests
{
    [TestMethod]
    public async Task GivenMemberExistsWhenLoadingDetailsThenSetsAllProperties()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberId = Guid.NewGuid();
        var members = new MemberSummary[]
        {
            new(memberId, "Test Member", "test@example.com", "Active", DateTimeOffset.UtcNow)
        };
        var checkIns = new MemberCheckIn[]
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow)
        };
        var classes = new FitnessClass[]
        {
            new(Guid.NewGuid(), "Yoga", DateTimeOffset.UtcNow.AddDays(1), 20)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(members, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetMemberCheckInsAsync(memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(checkIns, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetMemberClassesAsync(memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(classes, HttpStatusCode.OK));
        var model = new DetailsModel(apiClient.Object);

        // Act
        await model.OnGetAsync(memberId, cancellationToken);

        // Assert
        Assert.AreEqual(memberId, model.MemberId);
        Assert.IsNotNull(model.Member);
        Assert.AreEqual("Test Member", model.Member.Name);
        Assert.HasCount(1, model.CheckIns);
        Assert.HasCount(1, model.Classes);
        Assert.IsNull(model.Error);
    }

    [TestMethod]
    public async Task GivenMemberNotFoundWhenLoadingDetailsThenSetsError()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberId = Guid.NewGuid();
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<MemberSummary>(), HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetMemberCheckInsAsync(memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<MemberCheckIn>(), HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetMemberClassesAsync(memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<FitnessClass>(), HttpStatusCode.OK));
        var model = new DetailsModel(apiClient.Object);

        // Act
        await model.OnGetAsync(memberId, cancellationToken);

        // Assert
        Assert.IsNull(model.Member);
        Assert.IsNotNull(model.Error);
        Assert.AreEqual("Member.NotFound", model.Error.Code);
    }

    [TestMethod]
    public async Task GivenSuccessfulCheckInWhenPostingThenRedirectsWithSuccessMessage()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberId = Guid.NewGuid();
        var checkInCreated = new CheckInCreated(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.CheckInMemberAsync(memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(checkInCreated, HttpStatusCode.OK));
        var model = CreateModelWithTempData(apiClient.Object);

        // Act
        var result = await model.OnPostCheckInAsync(memberId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        var redirect = (RedirectToPageResult)result;
        Assert.AreEqual("Member checked in successfully.", model.TempData["Message"]);
        Assert.IsFalse(model.TempData.ContainsKey("Error"));
    }

    [TestMethod]
    public async Task GivenInactiveMemberWhenCheckingInThenRedirectsWithErrorMessage()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberId = Guid.NewGuid();
        var error = new ApiError("Member.Inactive", "Member is not active.", "op123");
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.CheckInMemberAsync(memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Failure<CheckInCreated>(error));
        var model = CreateModelWithTempData(apiClient.Object);

        // Act
        var result = await model.OnPostCheckInAsync(memberId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        Assert.AreEqual("Member is not active. Reference: op123", model.TempData["Error"]);
        Assert.IsFalse(model.TempData.ContainsKey("Message"));
    }

    [TestMethod]
    public async Task GivenNoFileSelectedWhenUploadingThenRedirectsWithErrorMessage()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberId = Guid.NewGuid();
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        var model = CreateModelWithTempData(apiClient.Object);
        model.Document = null;

        // Act
        var result = await model.OnPostUploadAsync(memberId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        Assert.AreEqual("Choose a document before uploading.", model.TempData["Error"]);
    }

    [TestMethod]
    public async Task GivenFileSelectedWhenUploadingThenCallsApiAndRedirects()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberId = Guid.NewGuid();
        var storageKey = "members/member/document";
        var uploaded = new MemberDocumentUploaded(storageKey);
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.UploadMemberDocumentAsync(
                memberId,
                "test.pdf",
                "application/pdf",
                It.IsAny<Stream>(),
                cancellationToken))
            .ReturnsAsync(ApiResult.Success(uploaded, HttpStatusCode.OK));
        var model = CreateModelWithTempData(apiClient.Object);
        var file = new Mock<IFormFile>(MockBehavior.Strict);
        file.Setup(f => f.FileName).Returns("test.pdf");
        file.Setup(f => f.ContentType).Returns("application/pdf");
        file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([1, 2, 3]));
        model.Document = file.Object;

        // Act
        var result = await model.OnPostUploadAsync(memberId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        Assert.AreEqual($"Document stored as {storageKey}.", model.TempData["Message"]);
        apiClient.Verify(
            client => client.UploadMemberDocumentAsync(
                memberId,
                "test.pdf",
                "application/pdf",
                It.IsAny<Stream>(),
                cancellationToken),
            Times.Once);
    }

    private static DetailsModel CreateModelWithTempData(IMembershipApiClient apiClient)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };
        var model = new DetailsModel(apiClient)
        {
            PageContext = pageContext,
            TempData = tempData
        };
        return model;
    }
}
