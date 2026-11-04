using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;


/// Viser matcher og lar aktørene foreslå koblinger mellom behov og ressurser.
/// Vanlige brukere ser egne matcher og må eie minst én relevant side
/// av et forslag. Administratorer har samlet oversikt.
/// Motparten kan godkjenne eller avslå et forslag. Admin kan administrere alle.

[Authorize(Roles = AppRoles.Admin + "," +
                   AppRoles.ResourceOwner + "," +
                   AppRoles.NeedsActor)]
public class MatchController : Controller
{
    private readonly IMatchRepository _matchRepository;
    private readonly INeedRepository _needRepository;
    private readonly IResourceRepository _resourceRepository;

    public MatchController(
        IMatchRepository matchRepository,
        INeedRepository needRepository,
        IResourceRepository resourceRepository)
    {
        _matchRepository = matchRepository;
        _needRepository = needRepository;
        _resourceRepository = resourceRepository;
    }


    /// Viser alle aktive matcher for admin og egne matcher for andre brukere.

    public async Task<IActionResult> Overview()
    {
        var matches = User.IsInRole(AppRoles.Admin)
            ? await _matchRepository.GetActiveAsync()
            : await _matchRepository.GetActiveForUserAsync(CurrentUserId);

        return View(PrepareMatchItems(matches));
    }


    /// Viser samlet historikk for admin og egen historikk for andre brukere.

    public async Task<IActionResult> History()
    {
        var matches = User.IsInRole(AppRoles.Admin)
            ? await _matchRepository.GetHistoryAsync()
            : await _matchRepository.GetHistoryForUserAsync(CurrentUserId);

        return View(PrepareMatchItems(matches));
    }


    /// Viser skjemaet for matchforslag med valg tilpasset brukerens rolle.

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateMatchViewModel();
        await LoadOptionsAsync(model);
        return View(model);
    }


    /// Kontrollerer registreringene og brukerens eierskap før et forslag lagres.
    /// Nye matcher får status Foreslått.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMatchViewModel model)
    {
        var need = await _needRepository.GetByIdAsync(model.NeedId);
        var resource = await _resourceRepository.GetByIdAsync(model.ResourceId);

        if (need is null)
        {
            ModelState.AddModelError(
                nameof(model.NeedId),
                "Behovet finnes ikke.");
        }

        if (resource is null)
        {
            ModelState.AddModelError(
                nameof(model.ResourceId),
                "Ressursen finnes ikke.");
        }

        // Kontrollerer eierskap mot databasen, ikke mot verdier fra skjemaet.
        if (need is not null &&
            resource is not null &&
            !CanProposeMatch(need, resource))
        {
            return Forbid();
        }

        if (await _matchRepository.ExistsAsync(model.NeedId, model.ResourceId))
        {
            ModelState.AddModelError(
                string.Empty,
                "Denne kombinasjonen er allerede matchet.");
        }

        if (need is not null && resource is not null)
        {
            try
            {
                MatchAvailability.Allocate(need, resource,
                    await _matchRepository.GetReservationsAsync(), model.AssignedQuantity);
            }
            catch (MatchAvailabilityException error)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(model);
            return View(model);
        }

        try
        {
            await _matchRepository.CreateAsync(new Match
            {
                NeedId = model.NeedId,
                ResourceId = model.ResourceId,
                AssignedQuantity = model.AssignedQuantity,
                // Opphavspersonen hentes fra innloggingen og kan ikke velges i skjemaet.
                ProposedByUserId = CurrentUserId,
                Status = MatchStatuses.Proposed,
                MatchedAt = DateTime.UtcNow,
                Notes = model.Notes
            });
        }
        catch (MatchAvailabilityException error)
        {
            // Tilgjengeligheten kan endres mellom visning og innsending.
            ModelState.AddModelError(string.Empty, error.Message);
            await LoadOptionsAsync(model);
            return View(model);
        }

        return RedirectToAction(nameof(Overview));
    }


    /// Motparten kan godkjenne eller avslå. Begge deltakere kan fullføre en godkjent match.
    /// Eierskap, rolle og forslagsstiller kontrolleres på serveren ved hvert svar.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!MatchStatuses.All.Contains(status)) return BadRequest();

        var match = await _matchRepository.GetByIdAsync(id);
        if (match is null) return NotFound();

        var need = await _needRepository.GetByIdAsync(match.NeedId);
        var resource = await _resourceRepository.GetByIdAsync(match.ResourceId);
        if (need is null || resource is null) return NotFound();

        var isResponse = status == MatchStatuses.Approved || status == MatchStatuses.Declined;
        var canUpdate = isResponse
            ? CanRespondToProposal(match.ProposedByUserId, need.UserId, resource.UserId)
            : status == MatchStatuses.Completed && CanCompleteMatch(need.UserId, resource.UserId);

        if (!canUpdate) return Forbid();

        var validTransition =
            (match.Status == MatchStatuses.Proposed &&
                (status == MatchStatuses.Approved ||
                 status == MatchStatuses.Declined)) ||
            (match.Status == MatchStatuses.Approved &&
                status == MatchStatuses.Completed) ||
            match.Status == status;

        if (!validTransition)
        {
            return BadRequest("Ugyldig statusovergang.");
        }

        var isClosed =
            status == MatchStatuses.Completed ||
            status == MatchStatuses.Declined;

        DateTime? resolvedAt = isClosed ? DateTime.UtcNow : null;

        // Gjentatt innsending av samme svar endrer ikke avslutningstidspunktet.
        try
        {
            if (match.Status != status &&
                !await _matchRepository.TryUpdateStatusAsync(id, match.Status, status, resolvedAt))
            {
                return Conflict("Matchen er allerede endret. Last oversikten på nytt.");
            }
        }
        catch (MatchAvailabilityException error)
        {
            return Conflict(error.Message);
        }

        // Et behandlet forslag hører til historikken, også når det bare er godkjent.
        return RedirectToAction(nameof(History));
    }

    
    /// Beregner knappetilgang for både forslagssiden og historikken.
    /// De samme eierskapskontrollene gjentas ved innsending av en statusendring.
 
    private List<MatchListItemViewModel> PrepareMatchItems(
        IEnumerable<MatchListItemViewModel> matches)
    {
        var items = matches.ToList();
        foreach (var item in items)
        {
            item.CanRespond = item.Status == MatchStatuses.Proposed &&
                CanRespondToProposal(item.ProposedByUserId, item.NeedUserId, item.ResourceUserId);
            item.CanComplete = item.Status == MatchStatuses.Approved &&
                CanCompleteMatch(item.NeedUserId, item.ResourceUserId);
        }

        return items;
    }


    /// Henter skjemavalg tilpasset rolle og eierskap.
    /// Behovsaktører velger egne behov, og ressurseiere velger egne ressurser.

    private async Task LoadOptionsAsync(CreateMatchViewModel model)
    {
        var needs = await _needRepository.GetAllAsync();
        var resources = await _resourceRepository.GetAllAsync();
        var reservations = (await _matchRepository.GetReservationsAsync()).ToList();
        needs = needs.Where(need => MatchAvailability.CanMatchNeed(need, reservations));
        resources = resources.Where(resource => MatchAvailability.CanMatchResource(resource, reservations));

        if (!User.IsInRole(AppRoles.Admin))
        {
            var userId = CurrentUserId;
            var isNeedsActor = User.IsInRole(AppRoles.NeedsActor);
            var isResourceOwner = User.IsInRole(AppRoles.ResourceOwner);

            if (isNeedsActor && !isResourceOwner)
            {
                needs = needs.Where(need => need.UserId == userId);
            }
            else if (isResourceOwner && !isNeedsActor)
            {
                resources = resources.Where(resource => resource.UserId == userId);
            }

            // Brukere med begge roller får begge listene.
            // Ved innsending kreves fortsatt eierskap til minst én side.
        }

        model.Needs = needs
            .Select(need => new SelectListItem(
                OptionText(need.Title, MatchAvailability.AvailableNeed(need, reservations), need.Unit),
                need.NeedId.ToString()))
            .ToList();

        model.Resources = resources
            .Select(resource => new SelectListItem(
                OptionText(resource.Title, MatchAvailability.AvailableResource(resource, reservations), resource.Unit),
                resource.ResourceId.ToString()))
            .ToList();
    }

    private static string OptionText(string title, decimal? quantity, string? unit) =>
        quantity.HasValue ? $"{title} – {quantity:0.##} {unit} ledig" : title;


    /// Admin kan foreslå alle kombinasjoner.
    /// Andre må eie behovet som behovsaktør eller ressursen som ressurseier.

    private bool CanProposeMatch(Need need, Resource resource)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        var userId = CurrentUserId;

        return
            (User.IsInRole(AppRoles.NeedsActor) && need.UserId == userId) ||
            (User.IsInRole(AppRoles.ResourceOwner) && resource.UserId == userId);
    }

  
    /// Bare motparten kan svare på et kjent forslag. Forslagsstilleren kan ikke
    /// godkjenne sitt eget forslag, selv om kontoen har begge aktørrollene.
    /// Forslag fra en admin uten eierskap sendes til ressurseieren.
    /// Eldre forslag uten kjent forslagsstiller kan bare behandles av admin.
  
    private bool CanRespondToProposal(
        string? proposedByUserId,
        string needUserId,
        string resourceUserId)
    {
        if (User.IsInRole(AppRoles.Admin)) return true;

        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(proposedByUserId) || proposedByUserId == userId)
        {
            return false;
        }

        if (proposedByUserId == resourceUserId)
        {
            return User.IsInRole(AppRoles.NeedsActor) && needUserId == userId;
        }

        return User.IsInRole(AppRoles.ResourceOwner) && resourceUserId == userId;
    }

   
    /// Begge parter kan markere en godkjent match som fullført.
    /// Admin har samme mulighet, men trengs ikke i den vanlige arbeidsflyten.
  
    private bool CanCompleteMatch(string needUserId, string resourceUserId) =>
        User.IsInRole(AppRoles.Admin) ||
        (User.IsInRole(AppRoles.NeedsActor) && needUserId == CurrentUserId) ||
        (User.IsInRole(AppRoles.ResourceOwner) && resourceUserId == CurrentUserId);


    /// Henter bruker-ID fra den aktive innloggingen.

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "Innlogget bruker mangler identifikator.");
}
