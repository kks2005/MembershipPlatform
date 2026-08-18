using MembershipPlatform.Application;
using MembershipPlatform.Application.Classes;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Moq;

namespace MembershipPlatform.Application.Tests.Classes;

[TestClass]
public sealed class RegisterMemberForClassTests
{
    [TestMethod]
    public async Task GivenValidRequestWhenRegisteringThenCreatesRegistration()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        var fitnessClass = CreateClass(capacity: 10);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        ClassRegistration? addedRegistration = null;
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        classRepository
            .Setup(repository => repository.GetByIdAsync(fitnessClass.ClassId, cancellationToken))
            .ReturnsAsync(fitnessClass);
        registrationRepository
            .Setup(repository => repository.ExistsAsync(
                member.MemberId,
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(false);
        registrationRepository
            .Setup(repository => repository.GetRegistrationCountAsync(
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(9);
        registrationRepository
            .Setup(repository => repository.TryAddAsync(
                It.IsAny<ClassRegistration>(),
                fitnessClass.Capacity,
                cancellationToken))
            .Callback<ClassRegistration, int, CancellationToken>(
                (registration, _, _) => addedRegistration = registration)
            .ReturnsAsync(ClassRegistrationAddResult.Added);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            member.MemberId,
            fitnessClass.ClassId,
            cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.ErrorCode);
        Assert.IsNull(result.ErrorMessage);
        Assert.IsNotNull(addedRegistration);
        Assert.AreEqual(member.MemberId, addedRegistration.MemberId);
        Assert.AreEqual(fitnessClass.ClassId, addedRegistration.ClassId);
        Assert.AreEqual(RegistrationStatus.Registered, addedRegistration.Status);
        Assert.AreEqual(addedRegistration.RegistrationId, result.RegistrationId);
        Assert.AreEqual(addedRegistration.RegisteredDate, result.RegisteredDate);
        registrationRepository.Verify(
            repository => repository.TryAddAsync(
                It.Is<ClassRegistration>(registration => registration == addedRegistration),
                fitnessClass.Capacity,
                cancellationToken),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenMemberDoesNotExistWhenRegisteringThenReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var classId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(memberId, cancellationToken))
            .ReturnsAsync((Member?)null);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(memberId, classId, cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.MemberNotFound, result.ErrorCode);
        Assert.AreEqual("Member not found.", result.ErrorMessage);
        VerifyAddWasNeverCalled(registrationRepository);
    }

    [TestMethod]
    public async Task GivenMemberIsInactiveWhenRegisteringThenReturnsFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Inactive);
        var classId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(member.MemberId, classId, cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.MemberInactive, result.ErrorCode);
        Assert.AreEqual("Member is not active.", result.ErrorMessage);
        VerifyAddWasNeverCalled(registrationRepository);
    }

    [TestMethod]
    public async Task GivenClassDoesNotExistWhenRegisteringThenReturnsFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        var classId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        classRepository
            .Setup(repository => repository.GetByIdAsync(classId, cancellationToken))
            .ReturnsAsync((FitnessClass?)null);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(member.MemberId, classId, cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.ClassNotFound, result.ErrorCode);
        Assert.AreEqual("Class not found.", result.ErrorMessage);
        VerifyAddWasNeverCalled(registrationRepository);
    }

    [TestMethod]
    public async Task GivenRegistrationAlreadyExistsWhenRegisteringThenReturnsFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        var fitnessClass = CreateClass(capacity: 10);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        classRepository
            .Setup(repository => repository.GetByIdAsync(fitnessClass.ClassId, cancellationToken))
            .ReturnsAsync(fitnessClass);
        registrationRepository
            .Setup(repository => repository.ExistsAsync(
                member.MemberId,
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(true);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            member.MemberId,
            fitnessClass.ClassId,
            cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.ClassAlreadyRegistered, result.ErrorCode);
        Assert.AreEqual(
            "Member is already registered for this class.",
            result.ErrorMessage);
        VerifyAddWasNeverCalled(registrationRepository);
    }

    [TestMethod]
    public async Task GivenClassIsAtCapacityWhenRegisteringThenReturnsFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        var fitnessClass = CreateClass(capacity: 10);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        classRepository
            .Setup(repository => repository.GetByIdAsync(fitnessClass.ClassId, cancellationToken))
            .ReturnsAsync(fitnessClass);
        registrationRepository
            .Setup(repository => repository.ExistsAsync(
                member.MemberId,
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(false);
        registrationRepository
            .Setup(repository => repository.GetRegistrationCountAsync(
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(fitnessClass.Capacity);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            member.MemberId,
            fitnessClass.ClassId,
            cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.ClassAtCapacity, result.ErrorCode);
        Assert.AreEqual("Class is at capacity.", result.ErrorMessage);
        VerifyAddWasNeverCalled(registrationRepository);
    }

    [TestMethod]
    public async Task GivenConcurrentDuplicateWhenSavingThenReturnsAlreadyRegisteredFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        var fitnessClass = CreateClass(capacity: 10);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        classRepository
            .Setup(repository => repository.GetByIdAsync(fitnessClass.ClassId, cancellationToken))
            .ReturnsAsync(fitnessClass);
        registrationRepository
            .Setup(repository => repository.ExistsAsync(
                member.MemberId,
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(false);
        registrationRepository
            .Setup(repository => repository.GetRegistrationCountAsync(
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(1);
        registrationRepository
            .Setup(repository => repository.TryAddAsync(
                It.IsAny<ClassRegistration>(),
                fitnessClass.Capacity,
                cancellationToken))
            .ReturnsAsync(ClassRegistrationAddResult.AlreadyRegistered);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            member.MemberId,
            fitnessClass.ClassId,
            cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.ClassAlreadyRegistered, result.ErrorCode);
        Assert.AreEqual(
            "Member is already registered for this class.",
            result.ErrorMessage);
    }

    [TestMethod]
    public async Task GivenConcurrentCapacityChangeWhenSavingThenReturnsAtCapacityFailure()
    {
        // Arrange
        var member = CreateMember(MemberStatus.Active);
        var fitnessClass = CreateClass(capacity: 10);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var memberRepository = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classRepository = new Mock<IClassRepository>(MockBehavior.Strict);
        var registrationRepository = new Mock<IClassRegistrationRepository>(MockBehavior.Strict);
        memberRepository
            .Setup(repository => repository.GetByIdAsync(member.MemberId, cancellationToken))
            .ReturnsAsync(member);
        classRepository
            .Setup(repository => repository.GetByIdAsync(fitnessClass.ClassId, cancellationToken))
            .ReturnsAsync(fitnessClass);
        registrationRepository
            .Setup(repository => repository.ExistsAsync(
                member.MemberId,
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(false);
        registrationRepository
            .Setup(repository => repository.GetRegistrationCountAsync(
                fitnessClass.ClassId,
                cancellationToken))
            .ReturnsAsync(9);
        registrationRepository
            .Setup(repository => repository.TryAddAsync(
                It.IsAny<ClassRegistration>(),
                fitnessClass.Capacity,
                cancellationToken))
            .ReturnsAsync(ClassRegistrationAddResult.AtCapacity);
        var useCase = new RegisterMemberForClass(
            memberRepository.Object,
            classRepository.Object,
            registrationRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            member.MemberId,
            fitnessClass.ClassId,
            cancellationToken);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApplicationErrorCodes.ClassAtCapacity, result.ErrorCode);
        Assert.AreEqual("Class is at capacity.", result.ErrorMessage);
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

    private static FitnessClass CreateClass(int capacity) =>
        new()
        {
            ClassId = Guid.NewGuid(),
            Name = "Test Class",
            StartTime = DateTimeOffset.UtcNow.AddDays(1),
            Capacity = capacity
        };

    private static void VerifyAddWasNeverCalled(
        Mock<IClassRegistrationRepository> registrationRepository) =>
        registrationRepository.Verify(
            repository => repository.TryAddAsync(
                It.IsAny<ClassRegistration>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
