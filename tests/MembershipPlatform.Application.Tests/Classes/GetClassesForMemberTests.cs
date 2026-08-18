using MembershipPlatform.Application.Classes;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Queries;
using Moq;

namespace MembershipPlatform.Application.Tests.Classes;

[TestClass]
public sealed class GetClassesForMemberTests
{
    [TestMethod]
    public async Task GivenMemberHasClassesWhenGettingClassesThenReturnsAllItems()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        IReadOnlyList<FitnessClass> classes =
        [
            CreateClass("Yoga", 20),
            CreateClass("Strength", 12)
        ];
        var query = new Mock<IMemberClassQuery>(MockBehavior.Strict);
        query
            .Setup(port => port.GetClassesForMemberAsync(memberId, cancellationToken))
            .ReturnsAsync(classes);
        var useCase = new GetClassesForMember(query.Object);

        // Act
        var result = await useCase.ExecuteAsync(memberId, cancellationToken);

        // Assert
        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(
            classes.Select(fitnessClass => fitnessClass.ClassId).ToArray(),
            result.Select(item => item.ClassId).ToArray());
        CollectionAssert.AreEquivalent(
            classes.Select(fitnessClass => fitnessClass.Name).ToArray(),
            result.Select(item => item.Name).ToArray());
        query.Verify(
            port => port.GetClassesForMemberAsync(memberId, cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenMemberHasNoClassesWhenGettingClassesThenReturnsEmptyCollection()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var query = new Mock<IMemberClassQuery>(MockBehavior.Strict);
        query
            .Setup(port => port.GetClassesForMemberAsync(memberId, cancellationToken))
            .ReturnsAsync([]);
        var useCase = new GetClassesForMember(query.Object);

        // Act
        var result = await useCase.ExecuteAsync(memberId, cancellationToken);

        // Assert
        Assert.IsEmpty(result);
        query.Verify(
            port => port.GetClassesForMemberAsync(memberId, cancellationToken),
            Times.Once);
    }

    private static FitnessClass CreateClass(string name, int capacity) =>
        new()
        {
            ClassId = Guid.NewGuid(),
            Name = name,
            StartTime = DateTimeOffset.UtcNow.AddDays(1),
            Capacity = capacity
        };
}
