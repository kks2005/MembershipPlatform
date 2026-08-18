using MembershipPlatform.Api.Contracts.Classes;
using MembershipPlatform.Api.Contracts.Common;
using MembershipPlatform.Application;
using MembershipPlatform.Application.Classes;
using Microsoft.AspNetCore.Mvc;

namespace MembershipPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/classes")]
public sealed class ClassesController(
    GetClasses getClasses,
    RegisterMemberForClass registerMemberForClass,
    GetMembersForClass getMembersForClass,
    GetClassRegistrationSummary getClassRegistrationSummary) : ControllerBase
{
    /// <summary>Gets all available fitness classes.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Returns all classes, or an empty collection.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
    {
        var classes = await getClasses.ExecuteAsync(cancellationToken);
        return Ok(classes.Select(item => new ClassResponse(
            item.ClassId,
            item.Name,
            item.StartTime,
            item.Capacity)).ToArray());
    }

    /// <summary>Gets members with active registrations for a class.</summary>
    /// <param name="classId">The class identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Returns the registered members.</response>
    [HttpGet("{classId:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(
        Guid classId,
        CancellationToken cancellationToken)
    {
        var members = await getMembersForClass.ExecuteAsync(classId, cancellationToken);
        return Ok(members.Select(item => new ClassMemberResponse(
            item.MemberId,
            item.Name,
            item.Email,
            item.Status.ToString(),
            item.JoinDate)).ToArray());
    }

    /// <summary>Gets registration counts and capacity for every class.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Returns a summary for every class.</response>
    [HttpGet("registration-summary")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ClassRegistrationSummaryResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRegistrationSummary(
        CancellationToken cancellationToken)
    {
        var summary = await getClassRegistrationSummary.ExecuteAsync(cancellationToken);
        return Ok(summary.Select(item => new ClassRegistrationSummaryResponse(
            item.ClassId,
            item.ClassName,
            item.Capacity,
            item.RegistrationCount)).ToArray());
    }

    /// <summary>Registers an active member for a class with available capacity.</summary>
    /// <param name="classId">The class identifier.</param>
    /// <param name="memberId">The member identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The class registration was created.</response>
    /// <response code="404">The member or class was not found.</response>
    /// <response code="409">The member is inactive, already registered, or the class is full.</response>
    [HttpPost("{classId:guid}/registrations/{memberId:guid}")]
    [ProducesResponseType(typeof(ClassRegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterMember(
        Guid classId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await registerMemberForClass.ExecuteAsync(
            memberId,
            classId,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new ClassRegistrationResponse(
                result.RegistrationId!.Value,
                result.RegisteredDate!.Value));
        }

        var error = new ApiErrorResponse(
            result.ErrorCode!,
            result.ErrorMessage!,
            HttpContext.TraceIdentifier);

        return result.ErrorCode switch
        {
            ApplicationErrorCodes.MemberNotFound => NotFound(error),
            ApplicationErrorCodes.ClassNotFound => NotFound(error),
            ApplicationErrorCodes.MemberInactive => Conflict(error),
            ApplicationErrorCodes.ClassAlreadyRegistered => Conflict(error),
            ApplicationErrorCodes.ClassAtCapacity => Conflict(error),
            _ => BadRequest(error)
        };
    }
}
