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
    private readonly INotificationRepository _notificationRepository;

    public MatchController(
        IMatchRepository matchRepository,
        INeedRepository needRepository,
        IResourceRepository resourceRepository
        INotificationRepository notificationRepository)
    {
        _matchRepository = matchRepository;
        _needRepository = needRepository;
        _resourceRepository = resourceRepository;
        _notificationRepository = notificationRepository;
    }


    /// Viser alle aktive matcher for admin og egne matcher for andre brukere.

    public async Task<IActionResult> Overview(string? search = null, string? status = null, bool requiresAction = false)
    {
        var matches = User.IsInRole(AppRoles.Admin)
            ? await _matchRepository.GetActiveAsync()
            : await _matchRepository.GetActiveForUserAsync(CurrentUserId);

        return View(FilterMatches(matches, search, status, requiresAction));
    }


    /// Viser samlet historikk for admin og egen historikk for andre brukere.

    public async Task<IActionResult> History(string? search = null, string? status = null, bool requiresAction = false)
    {
        var matches = User.IsInRole(AppRoles.Admin)
            ? await _matchRepository.GetHistoryAsync()
            : await _matchRepository.GetHistoryForUserAsync(CurrentUserId);

        ViewData["IsHistory"] = true;
        return View(FilterMatches(matches, search, status, requiresAction));
    }

    private List<MatchListItemViewModel> FilterMatches(IEnumerable<MatchListItemViewModel> matches, string? search, string? status, bool requiresAction)
    {
        var items = PrepareMatchItems(matches);
        ViewData["Search"] = search;
        ViewData["Status"] = status;
        ViewData["RequiresAction"] = requiresAction;
        ViewData["TotalMatches"] = items.Count;
        return MatchSearch.Apply(items, search, status, requiresAction).ToList();
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    public async Task<IActionResult> FindResources(int needId)
    {
        var need = await _needRepository.GetByIdAsync(needId);
        if (need is null) return NotFound();
        if (!MatchFlow.CanRequest(User, need)) return Forbid();
        var reservations = (await _matchRepository.GetReservationsAsync()).ToList();
        var model = new FindResourcesViewModel { Need = need };
        if (MatchAvailability.CanMatchNeed(need, reservations))
            foreach (var resource in await _resourceRepository.GetAllAsync())
                if (resource.UserId != need.UserId && ResourceSuitability.IsSuitable(need, resource, reservations) &&
                    !await _matchRepository.ExistsAsync(needId, resource.ResourceId))
                    model.Candidates.Add(new(resource, MatchAvailability.AvailableResource(resource, reservations)));
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    public async Task<IActionResult> DeliveryRequest(int needId, int resourceId)
    {
        var need = await _needRepository.GetByIdAsync(needId);
        if (need is null) return NotFound();
        if (!MatchFlow.CanRequest(User, need)) return Forbid();
        var resource = await _resourceRepository.GetByIdAsync(resourceId);
        if (resource is null) return NotFound();
        var reservations = (await _matchRepository.GetReservationsAsync()).ToList();
        if (resource.UserId == need.UserId || !ResourceSuitability.IsSuitable(need, resource, reservations))
            return BadRequest("Ressursen kan ikke dekke dette behovet. Gå tilbake og velg en annen ressurs.");
        var model = new DeliveryRequestViewModel
        {
            NeedId = needId, ResourceId = resourceId,
            AssignedQuantity = MatchAvailability.Allocate(need, resource, reservations, null),
            DeliveryAt = need.NeededFrom > DateTime.Now ? need.NeededFrom : DateTime.Now,
            DeliveryContact = need.ContactPoint ?? ""
        };
        PopulateRequest(model, need, resource, reservations);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    public async Task<IActionResult> DeliveryRequest(DeliveryRequestViewModel model)
    {
        var need = await _needRepository.GetByIdAsync(model.NeedId);
        if (need is null) return NotFound();
        if (!MatchFlow.CanRequest(User, need)) return Forbid();
        var resource = await _resourceRepository.GetByIdAsync(model.ResourceId);
        if (resource is null) return NotFound();
        var reservations = (await _matchRepository.GetReservationsAsync()).ToList();
        PopulateRequest(model, need, resource, reservations);
        if (resource.UserId == need.UserId) ModelState.AddModelError("", "Du kan ikke sende en forespørsel til egen konto.");
        if (ResourceSuitability.ResourceTypeFor(need.NeedTypeId) != resource.ResourceTypeId)
            ModelState.AddModelError("", "Ressurstypen passer ikke til behovet. Velg en ny ressurs.");
        if (model.DeliveryAt < DateTime.Now.AddMinutes(-5)) ModelState.AddModelError(nameof(model.DeliveryAt), "Velg et leveringstidspunkt nå eller frem i tid.");
        if ((resource.AvailableFrom.HasValue && model.DeliveryAt < resource.AvailableFrom) ||
            (resource.AvailableTo.HasValue && model.DeliveryAt > resource.AvailableTo))
            ModelState.AddModelError(nameof(model.DeliveryAt), "Tidspunktet må være innenfor ressursens tilgjengelighet.");
        if (await _matchRepository.ExistsAsync(model.NeedId, model.ResourceId)) ModelState.AddModelError("", "Det finnes allerede en aktiv forespørsel for denne kombinasjonen.");
        try
        {
            var quantity = MatchAvailability.Allocate(need, resource, reservations, model.AssignedQuantity);
            if (ModelState.IsValid)
            {
                await _matchRepository.CreateAsync(new Match
                {
                    NeedId = need.NeedId, ResourceId = resource.ResourceId, AssignedQuantity = quantity,
                    ProposedByUserId = CurrentUserId, Status = MatchStatuses.Proposed, MatchedAt = DateTime.UtcNow,
                    DeliveryAt = model.DeliveryAt, DeliveryLocation = model.DeliveryLocation.Trim(),
                    DeliveryContact = model.DeliveryContact.Trim(), Notes = model.Notes
                });

                // Varsler ressurseier om at ressurs er etterspurt.
                await _notificationRepository.CreateAsync(
                    resource.UserId,
                    $"Ny forespørsel: «{need.Title}» ønsker ressursen «{resource.Title}».",
                    "/Match/overview"); 
                
                TempData["DeliverySuccess"] = "Forespørselen er sendt. Venter på svar fra ressurseier.";
                return RedirectToAction(nameof(Overview));
            }
        }
        catch (MatchAvailabilityException error) { ModelState.AddModelError("", error.Message); }
        return View(model);
    }

    private static void PopulateRequest(DeliveryRequestViewModel model, Need need, Resource resource, IEnumerable<MatchReservation> reservations)
    {
        model.NeedTitle = need.Title;
        model.ResourceTitle = resource.Title;
        model.Unit = need.Unit ?? resource.Unit;
        model.SupplierContact = resource.ContactPoint;
        model.AvailableQuantity = MatchAvailability.AvailableResource(resource, reservations);
    }


    /// Viser skjemaet for matchforslag med valg tilpasset brukerens rolle.

    [HttpGet]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    public async Task<IActionResult> Create()
    {
        var model = new CreateMatchViewModel();
        await LoadOptionsAsync(model);
        return View(model);
    }


    /// Kontrollerer registreringene og brukerens eierskap før et forslag lagres.
    /// Nye matcher får status Foreslått.

    [HttpPost]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
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

        // Eldre skjemaer må også gjennomgå den konkrete leveringsavtalen.
        return RedirectToAction(nameof(DeliveryRequest), new { needId = model.NeedId, resourceId = model.ResourceId });
    }


    /// Ressurseier melder levert; offentlig aktør bekrefter deretter mottaket.
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
            : status switch
            {
                MatchStatuses.Delivered => MatchFlow.CanDeliver(User, resource.UserId),
                MatchStatuses.Completed => MatchFlow.CanReceive(User, need.UserId),
                MatchStatuses.Cancelled => MatchFlow.CanCancel(User, need.UserId, resource.UserId),
                _ => false
            };

        if (!canUpdate) return Forbid();

        var validTransition = MatchFlow.IsTransition(match.Status, status) || match.Status == status;

        if (!validTransition)
        {
            return BadRequest("Ugyldig statusovergang.");
        }

        var isClosed =
            status == MatchStatuses.Completed ||
            status == MatchStatuses.Declined || status == MatchStatuses.Cancelled;

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

        // Varsler behovseier når ressurseier har svart
        // match.Status er fortsatt den gamle verdien, så et gjentatt svar gir ikke nytt varsel.
        if (isResponse && match.Status != status && need.UserId != CurrentUserId)
        {
            var answer = status == MatchStatuses.Approved ? "godtatt" : "avslått";
            await _notificationRepository.CreateAsync(
                need.UserId,
                $"«{resource.Title}» har {answer} forespørselen din for «{need.Title}».",
                status == MatchStatuses.Approved ? "/Match/Overview" : "/Match/History");       
        }

        TempData["DeliverySuccess"] = status switch
        {
            MatchStatuses.Approved => "Levering er bekreftet. Mengden er reservert.",
            MatchStatuses.Delivered => "Leveransen er markert som levert. Venter på mottaksbekreftelse.",
            MatchStatuses.Completed => "Mottaket er bekreftet. Leveransen er fullført.",
            MatchStatuses.Cancelled => "Forespørselen er avbrutt. Eventuell reservasjon er frigitt.",
            _ => "Forespørselen er avslått."
        };
        return RedirectToAction(isClosed ? nameof(History) : nameof(Overview));
    }

    
    /// Beregner knappetilgang for både forslagssiden og historikken.
    /// De samme eierskapskontrollene gjentas ved innsending av en statusendring.
 
    private List<MatchListItemViewModel> PrepareMatchItems(
        IEnumerable<MatchListItemViewModel> matches)
    {
        var items = matches.ToList();
        foreach (var item in items)
        {
            MatchFlow.Prepare(User, item);
        }

        return items;
    }


    /// Henter skjemavalg tilpasset rolle og eierskap.
    /// Behovsaktører velger egne behov, og ressurseiere velger egne ressurser.

    private async Task LoadOptionsAsync(CreateMatchViewModel model)
    {
        var needs = await _needRepository.GetAllAsync();
        var resources = ResourceVisibility.Filter(User, await _resourceRepository.GetAllAsync());
        var reservations = (await _matchRepository.GetReservationsAsync()).ToList();
        needs = needs.Where(need => MatchAvailability.CanMatchNeed(need, reservations));
        resources = resources.Where(resource => MatchAvailability.CanMatchResource(resource, reservations));

        if (!User.IsInRole(AppRoles.Admin))
        {
            var userId = CurrentUserId;
            needs = needs.Where(need => need.UserId == userId);
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


    /// Offentlig aktør må eie behovet. Admin kan hjelpe på vegne av behovseieren.

    private bool CanProposeMatch(Need need, Resource resource)
    {
        return MatchFlow.CanRequest(User, need);
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

   
    /// Henter bruker-ID fra den aktive innloggingen.

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "Innlogget bruker mangler identifikator.");
}
