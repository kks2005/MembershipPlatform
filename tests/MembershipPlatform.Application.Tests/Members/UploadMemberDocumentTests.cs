using MembershipPlatform.Application;
using MembershipPlatform.Application.Members;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using MembershipPlatform.Core.Storage;
using Moq;

namespace MembershipPlatform.Application.Tests.Members;

[TestClass]
public sealed class UploadMemberDocumentTests
{
    [TestMethod]
    public async Task GivenMemberExistsWhenUploadingDocumentThenReturnsStorageKey()
    {
        // Arrange
        var member = CreateMember();
        using var content = new MemoryStream([1, 2, 3]);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var storage = new Mock<IMemberDocumentStorage>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        storage
            .Setup(adapter => adapter.SaveAsync(
                member.MemberId,
                "waiver.pdf",
                "application/pdf",
                content,
                cancellationToken))
            .ReturnsAsync(new MemberDocumentReference("members/member/document"));
        var useCase = new UploadMemberDocument(memberRepository.Object, storage.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            member.MemberId,
            "waiver.pdf",
            "application/pdf",
            content,
            cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("members/member/document", result.StorageKey);
        Assert.IsNull(result.ErrorCode);
        Assert.IsNull(result.ErrorMessage);
        memberRepository.Verify(
            repository => repository.GetByIdAsync(member.MemberId, cancellationToken),
            Times.Once);
        storage.Verify(
            adapter => adapter.SaveAsync(
                member.MemberId,
                "waiver.pdf",
                "application/pdf",
                content,
                cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenMemberDoesNotExistWhenUploadingDocumentThenStorageIsNotCalled()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        using var content = new MemoryStream([1, 2, 3]);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var storage = new Mock<IMemberDocumentStorage>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(memberId, cancellationToken))
            .ReturnsAsync((Member?)null);
        var useCase = new UploadMemberDocument(memberRepository.Object, storage.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            memberId,
            "waiver.pdf",
            "application/pdf",
            content,
            cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.MemberNotFound, result.ErrorCode);
        Assert.AreEqual("Member not found.", result.ErrorMessage);
        Assert.IsNull(result.StorageKey);
        storage.Verify(
            adapter => adapter.SaveAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Member CreateMember() =>
        new()
        {
            MemberId = Guid.NewGuid(),
            Name = "Test Member",
            Email = "member@example.com",
            Status = MemberStatus.Active,
            JoinDate = DateTimeOffset.UtcNow
        };
}
