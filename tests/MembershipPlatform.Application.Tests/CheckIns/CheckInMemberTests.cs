using MembershipPlatform.Application;
using MembershipPlatform.Application.CheckIns;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Moq;

namespace MembershipPlatform.Application.Tests.CheckIns;

[TestClass]
public sealed class CheckInMemberTests
{
    [TestMethod]
    public async Task GivenMemberDoesNotExistWhenCheckingInThenReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var checkInRepository = new Mock<ICheckInRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(memberId, cancellationToken))
            .ReturnsAsync((Member?)null);
        var useCase = new CheckInMember(memberRepository.Object, checkInRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(memberId, cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.MemberNotFound, result.ErrorCode);
        Assert.AreEqual("Member not found.", result.ErrorMessage);
        Assert.IsNull(result.CheckInId);
        Assert.IsNull(result.CheckInDate);
        memberRepository.Verify(
            repository => repository.GetByIdAsync(memberId, cancellationToken),
            Times.Once);
        checkInRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<CheckIn>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        memberRepository.VerifyNoOtherCalls();
        checkInRepository.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GivenMemberIsInactiveWhenCheckingInThenReturnsFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Inactive);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var checkInRepository = new Mock<ICheckInRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        var useCase = new CheckInMember(memberRepository.Object, checkInRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(member.MemberId, cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.MemberInactive, result.ErrorCode);
        Assert.AreEqual("Member is not active.", result.ErrorMessage);
        Assert.IsNull(result.CheckInId);
        Assert.IsNull(result.CheckInDate);
        memberRepository.Verify(
            repository => repository.GetByIdAsync(member.MemberId, cancellationToken),
            Times.Once);
        checkInRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<CheckIn>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        memberRepository.VerifyNoOtherCalls();
        checkInRepository.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GivenMemberIsActiveWhenCheckingInThenCreatesCheckIn()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var checkInRepository = new Mock<ICheckInRepository>(MockBehavior.Strict);
        CheckIn? addedCheckIn = null;
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        checkInRepository
            .Setup(repository => repository.AddAsync(It.IsAny<CheckIn>(), cancellationToken))
            .Callback<CheckIn, CancellationToken>((checkIn, _) => addedCheckIn = checkIn)
            .Returns(Task.CompletedTask);
        var useCase = new CheckInMember(memberRepository.Object, checkInRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(member.MemberId, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.ErrorCode);
        Assert.IsNull(result.ErrorMessage);
        Assert.IsNotNull(addedCheckIn);
        Assert.AreNotEqual(Guid.Empty, addedCheckIn.CheckInId);
        Assert.AreEqual(member.MemberId, addedCheckIn.MemberId);
        Assert.AreEqual(addedCheckIn.CheckInId, result.CheckInId);
        Assert.AreEqual(addedCheckIn.CheckInDate, result.CheckInDate);
        memberRepository.Verify(
            repository => repository.GetByIdAsync(member.MemberId, cancellationToken),
            Times.Once);
        checkInRepository.Verify(
            repository => repository.AddAsync(
                It.Is<CheckIn>(checkIn => checkIn == addedCheckIn),
                cancellationToken),
            Times.Once);
        memberRepository.VerifyNoOtherCalls();
        checkInRepository.VerifyNoOtherCalls();
    }

    private static Member CreateMember(MemberStatus status) =>
        new()
        {
            MemberId = Guid.NewGuid(),
            Name = "Test Member",
            Email = "member@example.com",
            Status = status,
            JoinDate = DateTimeOffset.UtcNow
        };

}
