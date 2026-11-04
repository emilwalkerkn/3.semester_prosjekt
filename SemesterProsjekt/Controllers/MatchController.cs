using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;


/// Håndterer opprettelse, oversikt og statusendringer for matcher
/// mellom behov og ressurser. Alle handlinger krever administratorrollen.
/// Databasetilgang skjer gjennom repositoryene.

[Authorize(Roles = AppRoles.Admin)]
public class MatchController : Controller
{
    private readonly IMatchRepository _matchRepository;
    private readonly INeedRepository _needRepository;
    private readonly IResourceRepository _resourceRepository;

  
    /// Mottar repositoryene gjennom dependency injection.
    /// Behovs- og ressursrepositoryene brukes til oppslag og skjemavalg,
    /// mens matchrepositoryet brukes til å hente og lagre matcher.
 
    public MatchController(
        IMatchRepository matchRepository,
        INeedRepository needRepository,
        IResourceRepository resourceRepository)
    {
        _matchRepository = matchRepository;
        _needRepository = needRepository;
        _resourceRepository = resourceRepository;
    }

  
    /// Henter aktive matcher og sender dem til matchoversikten.
 
    public async Task<IActionResult> Overview() =>
        View(await _matchRepository.GetActiveAsync());

  
    /// Henter avsluttede matcher og viser dem i historikken.
 
    public async Task<IActionResult> History() =>
        View(await _matchRepository.GetHistoryAsync());

  
    /// Viser et tomt skjema med behov og ressurser som kan velges
    /// når administratoren oppretter et matchforslag.
 
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateMatchViewModel();
        await LoadOptionsAsync(model);
        return View(model);
    }

  
    /// Kontrollerer skjemadata, at behovet og ressursen finnes,
    /// og om kombinasjonen allerede er registrert.
    /// En gyldig match lagres som et forslag med tidspunkt i UTC.
 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMatchViewModel model)
    {
        // ID-er fra skjemaet kontrolleres mot databasen.
        var need = await _needRepository.GetByIdAsync(model.NeedId);
        var resource = await _resourceRepository.GetByIdAsync(model.ResourceId);

        if (need is null)
            ModelState.AddModelError(
                nameof(model.NeedId),
                "Behovet finnes ikke.");

        if (resource is null)
            ModelState.AddModelError(
                nameof(model.ResourceId),
                "Ressursen finnes ikke.");

        if (await _matchRepository.ExistsAsync(model.NeedId, model.ResourceId))
            ModelState.AddModelError(
                string.Empty,
                "Denne kombinasjonen er allerede matchet.");

        if (!ModelState.IsValid)
        {
            // Nedtrekksvalgene må lastes på nytt når skjemaet vises med feil.
            await LoadOptionsAsync(model);
            return View(model);
        }

        await _matchRepository.CreateAsync(new Match
        {
            NeedId = model.NeedId,
            ResourceId = model.ResourceId,
            Status = MatchStatuses.Proposed,
            MatchedAt = DateTime.UtcNow,
            Notes = model.Notes
        });

        return RedirectToAction(nameof(Overview));
    }

  
    /// Oppdaterer status dersom verdien og overgangen er tillatt.
    /// Fullførte og avslåtte matcher får et avslutningstidspunkt,
    /// og brukeren videresendes til historikken.
 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        // Avviser statusverdier som ikke er definert av applikasjonen.
        if (!MatchStatuses.All.Contains(status)) return BadRequest();

        var match = await _matchRepository.GetByIdAsync(id);
        if (match is null) return NotFound();

        // Et forslag kan godkjennes eller avslås.
        // En godkjent match kan fullføres.
        // Innsending av samme status som den eksisterende godtas også.
        var validTransition =
            (match.Status == MatchStatuses.Proposed &&
                (status == MatchStatuses.Approved ||
                 status == MatchStatuses.Declined)) ||
            (match.Status == MatchStatuses.Approved &&
                status == MatchStatuses.Completed) ||
            match.Status == status;

        if (!validTransition)
            return BadRequest("Ugyldig statusovergang.");

        var isClosed =
            status == MatchStatuses.Completed ||
            status == MatchStatuses.Declined;

        DateTime? resolvedAt = isClosed ? DateTime.UtcNow : null;

        await _matchRepository.UpdateStatusAsync(id, status, resolvedAt);

        return RedirectToAction(
            isClosed ? nameof(History) : nameof(Overview));
    }

  
    /// Gjør behov og ressurser fra repositoryene om til nedtrekksvalg.
    /// Titlene vises til administratoren, mens ID-ene sendes inn med skjemaet.
 
    private async Task LoadOptionsAsync(CreateMatchViewModel model)
    {
        model.Needs = (await _needRepository.GetAllAsync())
            .Select(x => new SelectListItem(
                x.Title,
                x.NeedId.ToString()));

        model.Resources = (await _resourceRepository.GetAllAsync())
            .Select(x => new SelectListItem(
                x.Title,
                x.ResourceId.ToString()));
    }
}