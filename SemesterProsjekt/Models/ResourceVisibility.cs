using System.Security.Claims;

namespace SemesterProsjekt.Models;

// Samme leseregel brukes på oversikt, kart, utskrift og detaljsider.
public static class ResourceVisibility
{
    public static bool CanBrowseAll(ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.NeedsActor);

    public static string OverviewTitle(ClaimsPrincipal user) =>
        CanBrowseAll(user) ? "Tilgjengelige ressurser" : "Mine ressurser";

    public static bool CanView(ClaimsPrincipal user, Resource resource) =>
        CanBrowseAll(user) ||
        (user.IsInRole(AppRoles.ResourceOwner) &&
         user.FindFirstValue(ClaimTypes.NameIdentifier) is { } id && resource.UserId == id);

    public static IEnumerable<Resource> Filter(ClaimsPrincipal user, IEnumerable<Resource> resources) =>
        resources.Where(resource => CanView(user, resource));
}
