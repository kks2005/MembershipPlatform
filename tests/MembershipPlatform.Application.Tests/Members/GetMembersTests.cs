using MembershipPlatform.Application.Members;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Moq;

namespace MembershipPlatform.Application.Tests.Members;

[TestClass]
public sealed class GetMembersTests
{
    [TestMethod]
    public async Task GivenMultipleMembersWhenGettingMembersThenReturnsAllMemberDetails()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        IReadOnlyList<Member> members =
        [
            CreateMember("Maya Chen", MemberStatus.Active),
            CreateMember("Sofia Reyes", MemberStatus.Inactive)
        ];
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ReturnsAsync(members);
        var useCase = new GetMembers(memberRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.HasCount(2, result);
        Assert.AreEqual(members[0].MemberId, result[0].MemberId);
        Assert.AreEqual(members[0].Name, result[0].Name);
        Assert.AreEqual(members[0].Email, result[0].Email);
        Assert.AreEqual(members[0].Status, result[0].Status);
        Assert.AreEqual(members[0].JoinDate, result[0].JoinDate);
        Assert.AreEqual(members[1].Status, result[1].Status);
        memberRepository.Verify(
            repository => repository.GetAllAsync(cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenNoMembersWhenGettingMembersThenReturnsEmptyCollection()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ReturnsAsync([]);
        var useCase = new GetMembers(memberRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.IsEmpty(result);
        memberRepository.Verify(
            repository => repository.GetAllAsync(cancellationToken),
            Times.Once);
    }

    private static Member CreateMember(string name, MemberStatus status) =>
        new()
        {
            MemberId = Guid.NewGuid(),
            Name = name,
            Email = $"{name.Replace(' ', '.').ToLowerInvariant()}@example.com",
            Status = status,
            JoinDate = DateTimeOffset.UtcNow.AddMonths(-1)
        };
}
