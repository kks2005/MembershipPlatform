using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Application.Classes;

public sealed class RegisterMemberForClass(
    IMemberRepository memberRepository,
    IClassRepository classRepository,
    IClassRegistrationRepository classRegistrationRepository)
{
    public async Task<RegisterMemberForClassResult> ExecuteAsync(
        Guid memberId,
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

        if (member is null)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.MemberNotFound,
                "Member not found.");
        }

        if (member.Status != MemberStatus.Active)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.MemberInactive,
                "Member is not active.");
        }

        var fitnessClass = await classRepository.GetByIdAsync(classId, cancellationToken);

        if (fitnessClass is null)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.ClassNotFound,
                "Class not found.");
        }

        var registrationExists = await classRegistrationRepository.ExistsAsync(
            memberId,
            classId,
            cancellationToken);

        if (registrationExists)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.ClassAlreadyRegistered,
                "Member is already registered for this class.");
        }

        var registrationCount = await classRegistrationRepository.GetRegistrationCountAsync(
            classId,
            cancellationToken);

        if (registrationCount >= fitnessClass.Capacity)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.ClassAtCapacity,
                "Class is at capacity.");
        }

        var registration = new ClassRegistration
        {
            RegistrationId = Guid.NewGuid(),
            ClassId = classId,
            MemberId = memberId,
            RegisteredDate = DateTimeOffset.UtcNow,
            Status = RegistrationStatus.Registered
        };

        var addResult = await classRegistrationRepository.TryAddAsync(
            registration,
            fitnessClass.Capacity,
            cancellationToken);

        if (addResult == ClassRegistrationAddResult.AlreadyRegistered)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.ClassAlreadyRegistered,
                "Member is already registered for this class.");
        }

        if (addResult == ClassRegistrationAddResult.AtCapacity)
        {
            return RegisterMemberForClassResult.Failure(
                ApplicationErrorCodes.ClassAtCapacity,
                "Class is at capacity.");
        }

        if (addResult != ClassRegistrationAddResult.Added)
        {
            throw new InvalidOperationException(
                $"Unsupported class registration add result: {addResult}.");
        }

        return RegisterMemberForClassResult.Success(
            registration.RegistrationId,
            registration.RegisteredDate);
    }
}
