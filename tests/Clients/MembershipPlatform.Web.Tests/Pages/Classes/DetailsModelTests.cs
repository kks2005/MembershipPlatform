using System.Net;
using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using MembershipPlatform.Web.Pages.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace MembershipPlatform.Web.Tests.Pages.Classes;

[TestClass]
public sealed class DetailsModelTests
{
    [TestMethod]
    public async Task GivenClassExistsWhenLoadingDetailsThenSetsAllProperties()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var classes = new FitnessClass[]
        {
            new(classId, "Yoga", DateTimeOffset.UtcNow.AddDays(1), 20)
        };
        var members = new ClassMember[]
        {
            new(Guid.NewGuid(), "Test Member", "test@example.com", "Active", DateTimeOffset.UtcNow)
        };
        var availableMembers = new MemberSummary[]
        {
            new(Guid.NewGuid(), "Active Member", "active@example.com", "Active", DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Inactive Member", "inactive@example.com", "Inactive", DateTimeOffset.UtcNow)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(classes, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetClassMembersAsync(classId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(members, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(availableMembers, HttpStatusCode.OK));
        var model = new DetailsModel(apiClient.Object);

        // Act
        await model.OnGetAsync(classId, cancellationToken);

        // Assert
        Assert.AreEqual(classId, model.ClassId);
        Assert.IsNotNull(model.FitnessClass);
        Assert.AreEqual("Yoga", model.FitnessClass.Name);
        Assert.HasCount(1, model.Members);
        Assert.HasCount(2, model.AvailableMembers);
        Assert.AreEqual(availableMembers[0].MemberId, model.MemberId);
        Assert.IsNull(model.Error);
    }

    [TestMethod]
    public async Task GivenOnlyInactiveMembersWhenLoadingDetailsThenSelectsFirstMember()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var classes = new FitnessClass[]
        {
            new(classId, "Yoga", DateTimeOffset.UtcNow.AddDays(1), 20)
        };
        var availableMembers = new MemberSummary[]
        {
            new(Guid.NewGuid(), "Inactive Member", "inactive@example.com", "Inactive", DateTimeOffset.UtcNow)
        };
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.GetClassesAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(classes, HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetClassMembersAsync(classId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(Array.Empty<ClassMember>(), HttpStatusCode.OK));
        apiClient
            .Setup(client => client.GetMembersAsync(cancellationToken))
            .ReturnsAsync(ApiResult.Success(availableMembers, HttpStatusCode.OK));
        var model = new DetailsModel(apiClient.Object);

        // Act
        await model.OnGetAsync(classId, cancellationToken);

        // Assert
        Assert.AreEqual(availableMembers[0].MemberId, model.MemberId);
    }

    [TestMethod]
    public async Task GivenSuccessfulRegistrationWhenPostingThenRedirectsWithSuccessMessage()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var registrationCreated = new ClassRegistrationCreated(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.RegisterMemberAsync(classId, memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Success(registrationCreated, HttpStatusCode.OK));
        var model = CreateModelWithTempData(apiClient.Object);
        model.MemberId = memberId;

        // Act
        var result = await model.OnPostRegisterAsync(classId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        Assert.AreEqual("Member registered successfully.", model.TempData["Message"]);
    }

    [TestMethod]
    public async Task GivenInvalidModelStateWhenPostingThenRedirectsWithErrorMessage()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        var model = CreateModelWithTempData(apiClient.Object);
        model.MemberId = Guid.Empty;
        model.ModelState.AddModelError("MemberId", "Required");

        // Act
        var result = await model.OnPostRegisterAsync(classId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        Assert.AreEqual("Select a valid member.", model.TempData["Error"]);
    }

    [TestMethod]
    public async Task GivenDuplicateRegistrationWhenPostingThenRedirectsWithErrorMessage()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var error = new ApiError(
            "Class.AlreadyRegistered",
            "Member is already registered for this class.",
            "op123");
        var apiClient = new Mock<IMembershipApiClient>(MockBehavior.Strict);
        apiClient
            .Setup(client => client.RegisterMemberAsync(classId, memberId, cancellationToken))
            .ReturnsAsync(ApiResult.Failure<ClassRegistrationCreated>(error));
        var model = CreateModelWithTempData(apiClient.Object);
        model.MemberId = memberId;

        // Act
        var result = await model.OnPostRegisterAsync(classId, cancellationToken);

        // Assert
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
        Assert.AreEqual(
            "Member is already registered for this class. Reference: op123",
            model.TempData["Error"]);
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
