using MembershipPlatform.Application.Classes;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using Moq;

namespace MembershipPlatform.Application.Tests.Classes;

[TestClass]
public sealed class GetClassesTests
{
    [TestMethod]
    public async Task GivenMultipleClassesWhenGettingClassesThenReturnsAllItems()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        IReadOnlyList<FitnessClass> classes =
        [
            CreateClass("Strength", 12),
            CreateClass("Yoga", 20)
        ];
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        classRepository
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ReturnsAsync(classes);
        var useCase = new GetClasses(classRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(
            classes.Select(fitnessClass => fitnessClass.ClassId).ToArray(),
            result.Select(item => item.ClassId).ToArray());
        CollectionAssert.AreEquivalent(
            classes.Select(fitnessClass => fitnessClass.Name).ToArray(),
            result.Select(item => item.Name).ToArray());
        CollectionAssert.AreEquivalent(
            classes.Select(fitnessClass => fitnessClass.StartTime).ToArray(),
            result.Select(item => item.StartTime).ToArray());
        CollectionAssert.AreEquivalent(
            classes.Select(fitnessClass => fitnessClass.Capacity).ToArray(),
            result.Select(item => item.Capacity).ToArray());
        classRepository.Verify(
            repository => repository.GetAllAsync(cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenNoClassesWhenGettingClassesThenReturnsEmptyCollection()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        classRepository
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ReturnsAsync([]);
        var useCase = new GetClasses(classRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.IsEmpty(result);
        classRepository.Verify(
            repository => repository.GetAllAsync(cancellationToken),
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
