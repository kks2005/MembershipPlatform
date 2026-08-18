using MembershipPlatform.Api.Contracts.Classes;
using MembershipPlatform.Api.Contracts.Common;
using MembershipPlatform.Api.Contracts.Members;
using MembershipPlatform.Application;
using MembershipPlatform.Application.Classes;
using MembershipPlatform.Application.CheckIns;
using MembershipPlatform.Application.Members;
using Microsoft.AspNetCore.Mvc;

namespace MembershipPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/members")]
public sealed class MembersController(
    GetMembers getMembers,
    CheckInMember checkInMember,
    GetMemberCheckIns getMemberCheckIns,
    GetClassesForMember getClassesForMember) : ControllerBase
{
    /// <summary>Gets all members.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Returns all members, or an empty collection.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var members = await getMembers.ExecuteAsync(cancellationToken);
        return Ok(members.Select(member => new MemberResponse(
            member.MemberId,
            member.Name,
            member.Email,
            member.Status.ToString(),
            member.JoinDate)).ToArray());
    }

    /// <summary>Gets the classes for which a member has an active registration.</summary>
    /// <param name="memberId">The member identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Returns the member's registered classes.</response>
    [HttpGet("{memberId:guid}/classes")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClasses(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var classes = await getClassesForMember.ExecuteAsync(memberId, cancellationToken);
        return Ok(classes.Select(item => new ClassResponse(
            item.ClassId,
            item.Name,
            item.StartTime,
            item.Capacity)).ToArray());
    }

    /// <summary>Gets all check-ins recorded for a member.</summary>
    /// <param name="memberId">The member identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Returns the member's check-ins.</response>
    [HttpGet("{memberId:guid}/check-ins")]
    [ProducesResponseType(typeof(IReadOnlyList<MemberCheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCheckIns(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var checkIns = await getMemberCheckIns.ExecuteAsync(memberId, cancellationToken);
        return Ok(checkIns.Select(item => new MemberCheckInResponse(
            item.CheckInId,
            item.CheckInDate)).ToArray());
    }

    /// <summary>Checks an active member into the facility.</summary>
    /// <param name="memberId">The member identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The check-in was created.</response>
    /// <response code="404">The member was not found.</response>
    /// <response code="409">The member is inactive.</response>
    [HttpPost("{memberId:guid}/check-ins")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckIn(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await checkInMember.ExecuteAsync(memberId, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new CheckInResponse(
                result.CheckInId!.Value,
                result.CheckInDate!.Value));
        }

        var error = new ApiErrorResponse(
            result.ErrorCode!,
            result.ErrorMessage!,
            HttpContext.TraceIdentifier);

        return result.ErrorCode switch
        {
            ApplicationErrorCodes.MemberNotFound => NotFound(error),
            ApplicationErrorCodes.MemberInactive => Conflict(error),
            _ => BadRequest(error)
        };
    }
}
