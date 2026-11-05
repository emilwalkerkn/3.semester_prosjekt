using System.Security.Claims;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Models;

/// Styrer hvilke handlinger brukerne kan utføre på en match.
/// Kontrollerer roller, eierskap og hvilke statusendringer som er tillatt.
public static class MatchFlow
{
    // Henter ID-en til brukeren som er logget inn.
    private static string? Id(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    // Kontrollerer om brukeren kan opprette en forespørsel for et behov.
    // Administrator har tilgang til alle behov, mens andre må eie behovet.
    public static bool CanRequest(ClaimsPrincipal user, Need need) =>
        user.IsInRole(AppRoles.Admin) ||
        (user.IsInRole(AppRoles.NeedsActor) && need.UserId == Id(user));

    // Kontrollerer om brukeren kan svare på et matchforslag.
    // Forslaget må besvares av den andre parten, ikke av avsenderen selv.
    public static bool CanRespond(
        ClaimsPrincipal user,
        string? sender,
        string needOwner,
        string resourceOwner) =>
        user.IsInRole(AppRoles.Admin) ||
        (!string.IsNullOrWhiteSpace(sender) && sender != Id(user) &&
         (sender == resourceOwner
             ? user.IsInRole(AppRoles.NeedsActor) && needOwner == Id(user)
             : user.IsInRole(AppRoles.ResourceOwner) && resourceOwner == Id(user)));

    // Bare administrator eller eieren av ressursen kan bekrefte levering.
    public static bool CanDeliver(ClaimsPrincipal user, string resourceOwner) =>
        user.IsInRole(AppRoles.Admin) ||
        (user.IsInRole(AppRoles.ResourceOwner) && resourceOwner == Id(user));

    // Bare administrator eller eieren av behovet kan bekrefte mottak.
    public static bool CanReceive(ClaimsPrincipal user, string needOwner) =>
        user.IsInRole(AppRoles.Admin) ||
        (user.IsInRole(AppRoles.NeedsActor) && needOwner == Id(user));

    // Kontrollerer om brukeren har rett til å avbryte en match.
    public static bool CanCancel(
        ClaimsPrincipal user,
        string needOwner,
        string resourceOwner) =>
        CanReceive(user, needOwner) || CanDeliver(user, resourceOwner);

    // Bestemmer hvilke statusendringer som er tillatt i matchprosessen.
    // En match må følge riktig rekkefølge fra forslag til fullført levering.
    public static bool IsTransition(string from, string to) => (from, to) switch
    {
        (MatchStatuses.Proposed,
            MatchStatuses.Approved or MatchStatuses.Declined or MatchStatuses.Cancelled) => true,

        (MatchStatuses.Approved,
            MatchStatuses.Delivered or MatchStatuses.Cancelled) => true,

        (MatchStatuses.Delivered, MatchStatuses.Completed) => true,

        _ => false
    };

    /// Forbereder en match for visning på nettsiden.
    /// Bestemmer hvilke knapper brukeren kan se og hva neste steg er.
    public static void Prepare(ClaimsPrincipal user, MatchListItemViewModel item)
    {
        // Kontrollerer om brukeren kan svare på et nytt forslag.
        item.CanRespond =
            item.Status == MatchStatuses.Proposed &&
            CanRespond(user, item.ProposedByUserId, item.NeedUserId, item.ResourceUserId);

        // Kontrollerer om ressurseieren kan markere matchen som levert.
        item.CanDeliver =
            item.Status == MatchStatuses.Approved &&
            CanDeliver(user, item.ResourceUserId);

        // Kontrollerer om behovseieren kan bekrefte mottatt levering.
        item.CanComplete =
            item.Status == MatchStatuses.Delivered &&
            CanReceive(user, item.NeedUserId);

        // Kontrollerer hvem som kan avbryte en match.
        // Ved forslag kan avsenderen avbryte, mens begge parter kan avbryte etter godkjenning.
        item.CanCancel =
            (item.Status == MatchStatuses.Proposed &&
             (user.IsInRole(AppRoles.Admin) || item.ProposedByUserId == Id(user))) ||
            (item.Status == MatchStatuses.Approved &&
             CanCancel(user, item.NeedUserId, item.ResourceUserId));

        // Viser en beskrivelse av neste steg basert på status og brukerens rettigheter.
        item.NextStep = item.Status switch
        {
            MatchStatuses.Proposed =>
                item.CanRespond
                    ? "Bekreft levering eller avslå forespørselen"
                    : "Venter på svar fra ressurseier",

            MatchStatuses.Approved =>
                item.CanDeliver
                    ? "Marker som levert når leveransen er gjennomført"
                    : "Venter på levering fra ressurseier",

            MatchStatuses.Delivered =>
                item.CanComplete
                    ? "Bekreft at leveransen er mottatt"
                    : "Venter på mottaksbekreftelse fra offentlig aktør",

            _ => MatchStatuses.DisplayName(item.Status)
        };
    }
}