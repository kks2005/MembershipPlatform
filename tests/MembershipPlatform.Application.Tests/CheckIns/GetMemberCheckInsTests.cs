using MembershipPlatform.Application.CheckIns;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using Moq;

namespace MembershipPlatform.Application.Tests.CheckIns;

[TestClass]
public sealed class GetMemberCheckInsTests
{
    [TestMethod]
    public async Task GivenMemberHasMultipleCheckInsWhenGettingCheckInsThenReturnsAllItems()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        IReadOnlyList<CheckIn> checkIns =
        [
            new CheckIn
            {
                CheckInId = Guid.NewGuid(),
                MemberId = memberId,
                CheckInDate = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new CheckIn
            {
                CheckInId = Guid.NewGuid(),
                MemberId = memberId,
                CheckInDate = DateTimeOffset.UtcNow
            }
        ];
        var checkInRepository = new Mock<ICheckInRepository>(MockBehavior.Strict);
        checkInRepository
            .Setup(repository => repository.GetByMemberIdAsync(memberId, cancellationToken))
            .ReturnsAsync(checkIns);
        var useCase = new GetMemberCheckIns(checkInRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(memberId, cancellationToken);

        // Assert
        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(
            checkIns.Select(checkIn => checkIn.CheckInId).ToArray(),
            result.Select(item => item.CheckInId).ToArray());
        CollectionAssert.AreEquivalent(
            checkIns.Select(checkIn => checkIn.CheckInDate).ToArray(),
            result.Select(item => item.CheckInDate).ToArray());
        checkInRepository.Verify(
            repository => repository.GetByMemberIdAsync(memberId, cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenMemberHasNoCheckInsWhenGettingCheckInsThenReturnsEmptyList()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var checkInRepository = new Mock<ICheckInRepository>(MockBehavior.Strict);
        checkInRepository
            .Setup(repository => repository.GetByMemberIdAsync(memberId, cancellationToken))
            .ReturnsAsync([]);
        var useCase = new GetMemberCheckIns(checkInRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(memberId, cancellationToken);

        // Assert
        Assert.IsEmpty(result);
        checkInRepository.Verify(
            repository => repository.GetByMemberIdAsync(memberId, cancellationToken),
            Times.Once);
    }
}
