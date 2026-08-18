using MembershipPlatform.Application.Classes;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using Moq;

namespace MembershipPlatform.Application.Tests.Classes;

[TestClass]
public sealed class GetMembersForClassTests
{
    [TestMethod]
    public async Task GivenClassHasMembersWhenGettingMembersThenReturnsAllItems()
    {
        // Arrange
        var classId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        IReadOnlyList<Member> members =
        [
            CreateMember("First Member"),
            CreateMember("Second Member")
        ];
        var query = new Mock<IClassRegistrationQuery>(MockBehavior.Strict);
        query
            .Setup(port => port.GetMembersForClassAsync(classId, cancellationToken))
            .ReturnsAsync(members);
        var useCase = new GetMembersForClass(query.Object);

        // Act
        var result = await useCase.ExecuteAsync(classId, cancellationToken);

        // Assert
        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(
            members.Select(member => member.MemberId).ToArray(),
            result.Select(item => item.MemberId).ToArray());
        CollectionAssert.AreEquivalent(
            members.Select(member => member.Email).ToArray(),
            result.Select(item => item.Email).ToArray());
        query.Verify(
            port => port.GetMembersForClassAsync(classId, cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenClassHasNoMembersWhenGettingMembersThenReturnsEmptyCollection()
    {
        // Arrange
        var classId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var query = new Mock<IClassRegistrationQuery>(MockBehavior.Strict);
        query
            .Setup(port => port.GetMembersForClassAsync(classId, cancellationToken))
            .ReturnsAsync([]);
        var useCase = new GetMembersForClass(query.Object);

        // Act
        var result = await useCase.ExecuteAsync(classId, cancellationToken);

        // Assert
        Assert.IsEmpty(result);
        query.Verify(
            port => port.GetMembersForClassAsync(classId, cancellationToken),
            Times.Once);
    }

    private static Member CreateMember(string name) =>
        new()
        {
            MemberId = Guid.NewGuid(),
            Name = name,
            Email = $"{name.Replace(' ', '.')}@example.com",
            Status = MemberStatus.Active,
            JoinDate = DateTimeOffset.UtcNow
        };
}
