using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Classes;

/// <summary>
/// Page model for class details with member registration functionality.
/// Demonstrates: 
/// - Complex business rule enforcement (active members, capacity, duplicates)
/// - Form handling with dropdown selection
/// - Smart defaults (pre-select first active member)
/// </summary>
public sealed class DetailsModel(IMembershipApiClient apiClient) : PageModel
{
    // Properties exposed to the Razor view
    public Guid ClassId { get; private set; }
    public FitnessClass? FitnessClass { get; private set; }
    public IReadOnlyList<ClassMember> Members { get; private set; } = [];
    public IReadOnlyList<MemberSummary> AvailableMembers { get; private set; } = [];
    public ApiError? Error { get; private set; }

    /// <summary>
    /// Member ID selected in the dropdown form.
    /// The [BindProperty] attribute enables two-way binding with the form.
    /// </summary>
    [BindProperty]
    public Guid MemberId { get; set; }

    /// <summary>
    /// Handles GET requests to load class details.
    /// Demonstrates:
    /// - Three parallel API calls for efficiency
    /// - Finding a specific class from a collection
    /// - Smart default selection (prefer active members)
    /// </summary>
    public async Task OnGetAsync(Guid classId, CancellationToken cancellationToken)
    {
        ClassId = classId;

        // Execute three API calls in parallel
        // We fetch all classes (not just this one) because the API doesn't have
        // a single-class endpoint - this demonstrates working with collection endpoints
        var classesTask = apiClient.GetClassesAsync(cancellationToken);
        var classMembersTask = apiClient.GetClassMembersAsync(classId, cancellationToken);
        var availableMembersTask = apiClient.GetMembersAsync(cancellationToken);
        await Task.WhenAll(classesTask, classMembersTask, availableMembersTask);

        var classesResult = await classesTask;
        var classMembersResult = await classMembersTask;
        var availableMembersResult = await availableMembersTask;

        // Check for errors from any of the API calls
        Error = classesResult.Error ?? classMembersResult.Error ?? availableMembersResult.Error;

        if (Error is null)
        {
            // Find the specific class from the classes collection
            FitnessClass = classesResult.Value!.SingleOrDefault(item => item.ClassId == classId);

            // Get currently registered members
            Members = classMembersResult.Value!;

            // Get all available members for the registration dropdown
            // Note: This includes inactive and already-registered members intentionally
            // to demonstrate the API's rejection paths
            AvailableMembers = availableMembersResult.Value!;

            // Smart default: pre-select the first active member in the dropdown
            // This improves UX by showing a valid selection by default
            var defaultMember = AvailableMembers.FirstOrDefault(member =>
                string.Equals(member.Status, "Active", StringComparison.OrdinalIgnoreCase))
                ?? (AvailableMembers.Count > 0 ? AvailableMembers[0] : null);

            MemberId = defaultMember?.MemberId ?? Guid.Empty;
        }
    }

    /// <summary>
    /// Handles POST requests to register a member for the class.
    /// Demonstrates:
    /// - POST-Redirect-GET pattern (prevents duplicate submissions)
    /// - Model validation
    /// - Multiple business rule checks by the API:
    ///   * Member must be active (rejects inactive members)
    ///   * Class must have capacity (rejects when full)
    ///   * Member cannot be registered twice (rejects duplicates)
    /// - Error handling with operation ID correlation
    /// </summary>
    public async Task<IActionResult> OnPostRegisterAsync(
        Guid classId,
        CancellationToken cancellationToken)
    {
        // Validate the form submission
        if (!ModelState.IsValid || MemberId == Guid.Empty)
        {
            TempData["Error"] = "Select a valid member.";
            return RedirectToPage(new { classId });
        }

        // Call the API to register the member
        var result = await apiClient.RegisterMemberAsync(
            classId,
            MemberId,
            cancellationToken);

        if (result.IsSuccess)
        {
            // Registration succeeded
            TempData["Message"] = "Member registered successfully.";
        }
        else
        {
            // Registration failed - show the error message
            // Include operation ID if available for correlation with server logs
            TempData["Error"] = string.IsNullOrWhiteSpace(result.Error!.OperationId)
                ? result.Error.Message
                : $"{result.Error.Message} Reference: {result.Error.OperationId}";
        }

        // Redirect back to GET handler to refresh the page
        // This follows the POST-Redirect-GET pattern to prevent duplicate form submissions
        return RedirectToPage(new { classId });
    }
}
