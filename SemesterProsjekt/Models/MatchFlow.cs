using System.Security.Claims;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Models;

public static class MatchFlow
{
    private static string? Id(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
    public static bool CanRequest(ClaimsPrincipal user, Need need) => user.IsInRole(AppRoles.Admin) ||
        (user.IsInRole(AppRoles.NeedsActor) && need.UserId == Id(user));
    public static bool CanRespond(ClaimsPrincipal user, string? sender, string needOwner, string resourceOwner) =>
        user.IsInRole(AppRoles.Admin) || (!string.IsNullOrWhiteSpace(sender) && sender != Id(user) &&
        (sender == resourceOwner
            ? user.IsInRole(AppRoles.NeedsActor) && needOwner == Id(user)
            : user.IsInRole(AppRoles.ResourceOwner) && resourceOwner == Id(user)));
    public static bool CanDeliver(ClaimsPrincipal user, string resourceOwner) => user.IsInRole(AppRoles.Admin) ||
        (user.IsInRole(AppRoles.ResourceOwner) && resourceOwner == Id(user));
    public static bool CanReceive(ClaimsPrincipal user, string needOwner) => user.IsInRole(AppRoles.Admin) ||
        (user.IsInRole(AppRoles.NeedsActor) && needOwner == Id(user));
    public static bool CanCancel(ClaimsPrincipal user, string needOwner, string resourceOwner) =>
        CanReceive(user, needOwner) || CanDeliver(user, resourceOwner);
    public static bool IsTransition(string from, string to) => (from, to) switch
    {
        (MatchStatuses.Proposed, MatchStatuses.Approved or MatchStatuses.Declined or MatchStatuses.Cancelled) => true,
        (MatchStatuses.Approved, MatchStatuses.Delivered or MatchStatuses.Cancelled) => true,
        (MatchStatuses.Delivered, MatchStatuses.Completed) => true,
        _ => false
    };

    public static void Prepare(ClaimsPrincipal user, MatchListItemViewModel item)
    {
        item.CanRespond = item.Status == MatchStatuses.Proposed && CanRespond(user, item.ProposedByUserId, item.NeedUserId, item.ResourceUserId);
        item.CanDeliver = item.Status == MatchStatuses.Approved && CanDeliver(user, item.ResourceUserId);
        item.CanComplete = item.Status == MatchStatuses.Delivered && CanReceive(user, item.NeedUserId);
        item.CanCancel = (item.Status == MatchStatuses.Proposed && (user.IsInRole(AppRoles.Admin) || item.ProposedByUserId == Id(user))) ||
            (item.Status == MatchStatuses.Approved && CanCancel(user, item.NeedUserId, item.ResourceUserId));
        item.NextStep = item.Status switch
        {
            MatchStatuses.Proposed => item.CanRespond ? "Bekreft levering eller avslå forespørselen" : "Venter på svar fra ressurseier",
            MatchStatuses.Approved => item.CanDeliver ? "Marker som levert når leveransen er gjennomført" : "Venter på levering fra ressurseier",
            MatchStatuses.Delivered => item.CanComplete ? "Bekreft at leveransen er mottatt" : "Venter på mottaksbekreftelse fra offentlig aktør",
            _ => MatchStatuses.DisplayName(item.Status)
        };
    }
}
