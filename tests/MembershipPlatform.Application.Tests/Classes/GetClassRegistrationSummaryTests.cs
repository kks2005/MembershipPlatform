using MembershipPlatform.Application.Classes;
using MembershipPlatform.Core.Queries;
using Moq;

namespace MembershipPlatform.Application.Tests.Classes;

[TestClass]
public sealed class GetClassRegistrationSummaryTests
{
    [TestMethod]
    public async Task GivenRegistrationsExistWhenGettingSummaryThenReturnsAllItems()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        IReadOnlyList<ClassRegistrationSummary> summaries =
        [
            new(Guid.NewGuid(), "Yoga", 20, 8),
            new(Guid.NewGuid(), "Strength", 12, 5)
        ];
        var query = new Mock<IClassRegistrationQuery>(MockBehavior.Strict);
        query
            .Setup(port => port.GetClassRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync(summaries);
        var useCase = new GetClassRegistrationSummary(query.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(
            summaries.Select(summary => summary.ClassId).ToArray(),
            result.Select(item => item.ClassId).ToArray());
        CollectionAssert.AreEquivalent(
            summaries.Select(summary => summary.RegistrationCount).ToArray(),
            result.Select(item => item.RegistrationCount).ToArray());
        query.Verify(
            port => port.GetClassRegistrationSummaryAsync(cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenNoRegistrationsWhenGettingSummaryThenReturnsEmptyCollection()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var query = new Mock<IClassRegistrationQuery>(MockBehavior.Strict);
        query
            .Setup(port => port.GetClassRegistrationSummaryAsync(cancellationToken))
            .ReturnsAsync([]);
        var useCase = new GetClassRegistrationSummary(query.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.IsEmpty(result);
        query.Verify(
            port => port.GetClassRegistrationSummaryAsync(cancellationToken),
            Times.Once);
    }
}
