using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;


/// Håndterer visning og behandling av behov for innloggede brukere.
/// Databasetilgang skjer gjennom INeedRepository.
/// Endringer krever riktig rolle og at brukeren eier behovet eller er administrator.


[Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
public class NeedController : Controller
{
    private readonly INeedRepository _needRepository;
    
    
    /// Mottar repositoryet gjennom dependency injection, slik at controlleren
    /// kan hente og lagre behov uten å håndtere SQL eller databasetilkoblinger.

    public NeedController(INeedRepository needRepository)
    {
        _needRepository = needRepository;
    }
    
    
    /// Viser aktive behov eller historikk basert på valgt visning.
    /// Bruker samme søk, filtrering og kart for begge visningene.
    public async Task<IActionResult> Overview(
        string sortBy = "newest",
        string? search = null,
        int? needTypeId = null,
        string? status = null,
        string? priority = null,
        bool history = false)
    {
        // Henter behov fra databasen.
        var needs = (await _needRepository.GetAllAsync(sortBy)).ToList();

        // Viser bare løste behov i historikken og aktive behov ellers.
        needs = needs
            .Where(n => history
                ? n.Status == NeedStatuses.Resolved
                : n.Status != NeedStatuses.Resolved)
            .ToList();

        // Beholder valgte filtre i grensesnittet.
        ViewData["IsHistory"] = history;
        ViewData["SortBy"] = sortBy;
        ViewData["Search"] = search;
        ViewData["NeedTypeId"] = needTypeId;
        ViewData["Status"] = status;
        ViewData["Priority"] = priority;

        // Henter behovstyper til filtreringsmenyen.
        ViewData["NeedTypes"] = (await _needRepository.GetNeedTypesAsync())
            .Select(type => new SelectListItem(
                type.Name,
                type.NeedTypeId.ToString(),
                type.NeedTypeId == needTypeId))
            .OrderBy(item => item.Value == CategoryCatalog.OtherId.ToString())
            .ThenBy(item => item.Text)
            .ToList();

        // Bruker eksisterende søk og filtrering.
        return View(NeedSearch.Apply(
            needs,
            search,
            needTypeId,
            history ? null : status,
            priority).ToList());
    }


    
    
    /// Viser et tomt registreringsskjema med behovstyper hentet fra databasen.
    /// Bare behovsaktører og administratorer kan registrere behov.

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new NeedViewModel { NeededFrom = DateTime.Now };
        await LoadNeedTypesAsync(model);
        return View(model);
    }
    
    
    /// Validerer skjemaet og lagrer et nytt behov på den innloggede brukeren.
    /// Ved vellykket lagring videresendes brukeren til bekreftelsessiden.

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Nedtrekksvalgene må lastes på nytt når skjemaet vises med feil.
            await LoadNeedTypesAsync(model);
            return View(model);
        }

        model.NeededFrom ??= DateTime.Now;
        model.Deadline = null;
        var need = MapNewNeed(model);
        var id = await _needRepository.CreateAsync(need);
        return RedirectToAction(nameof(Result), new { id });
    }

    
    /// Viser bekreftelsen for et registrert behov.
    public async Task<IActionResult> Result(int id) =>
        await ShowNeedAsync(id, nameof(Result));

    
    /// Viser detaljene for et behov.
    public async Task<IActionResult> Details(int id) =>
        await ShowNeedAsync(id, nameof(Details));

    
    /// Henter et eksisterende behov og viser redigeringsskjemaet.
    /// Bare eieren eller en administrator med riktig rolle får tilgang.
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        if (!CanManage(need.UserId)) return Forbid();

        var model = ToViewModel(need);
        await LoadNeedTypesAsync(model);
        return View(model);
    }

    
    /// Validerer og lagrer endringer på et eksisterende behov.
    /// Kontrollerer både behovets ID og brukerens tilgang før oppdatering.
    /// 
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NeedViewModel model)
    {
        // Adressen og skjemaet må vise til samme behov.
        if (id != model.NeedId) return BadRequest();

        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        
        // Tilgangen kontrolleres også ved innsending, ikke bare når skjemaet åpnes.
        if (!CanManage(need.UserId)) return Forbid();

        // Status og eldre frist endres ikke gjennom registreringsfeltene.
        model.Status = need.Status;
        model.Deadline = need.Deadline;
        model.NeededFrom ??= need.NeededFrom;
        if (!ModelState.IsValid)
        {
            await LoadNeedTypesAsync(model);
            return View(model);
        }

        Apply(model, need);
        await _needRepository.UpdateAsync(need);
        return RedirectToAction(nameof(Details), new { id });
    }

    
    /// Viser en bekreftelsesside før sletting.
    /// Denne GET-forespørselen sletter ikke data.
    /// 
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        return CanManage(need.UserId) ? View(need) : Forbid();
    }
    
    /// Sletter behovet etter bekreftelse og ny kontroll av brukerens tilgang.
    /// Metoden mottar POST-forespørsler til Delete.

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        if (!CanManage(need.UserId)) return Forbid();

        await _needRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Overview));
    }
     
    
    /// Oppdaterer statusen til et behov.
    /// Løste behov kan ikke aktiveres på nytt.
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        // Kontrollerer at statusen er gyldig.
        if (!NeedStatuses.All.Contains(status))
            return BadRequest();

        // Henter behovet fra databasen.
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
            return NotFound();

        // Kontrollerer at brukeren har lov til å endre behovet.
        if (!CanManage(need.UserId))
            return Forbid();

        // Hindrer at avsluttede behov blir aktive igjen.
        if (need.Status == NeedStatuses.Resolved)
            return BadRequest("Løste behov kan ikke aktiveres på nytt.");

        // Oppdaterer statusen.
        await _needRepository.UpdateStatusAsync(id, status);

        return RedirectToAction(nameof(Overview));
    }


    
    /// Felles oppslag for detalj- og bekreftelsessidene.
    /// Returnerer HTTP 404 dersom behovet ikke finnes.
    /// 
    private async Task<IActionResult> ShowNeedAsync(int id, string viewName)
    {
        var need = await _needRepository.GetByIdAsync(id);
        return need is null ? NotFound() : View(viewName, need);
    }

    
    /// Gjør behovstypene fra databasen om til valg i skjemaets nedtrekksliste.
    /// Typens navn vises til brukeren, mens ID-en sendes inn med skjemaet.
    /// 
    private async Task LoadNeedTypesAsync(NeedViewModel model)
    {
        model.NeedTypes = (await _needRepository.GetNeedTypesAsync())
            .Select(x => new SelectListItem(x.Name, x.NeedTypeId.ToString()));
    }

    
    /// Oppretter en behovsmodell fra skjemaet og setter den innloggede brukeren som eier.
    /// Eierens ID hentes fra innloggingen, slik at den ikke kan velges i skjemaet.
    ///
    private Need MapNewNeed(NeedViewModel model)
    {
        var need = new Need { UserId = CurrentUserId };
        Apply(model, need);
        return need;
    }

    /// Overfører et lagret behov til modellen som brukes i redigeringsskjemaet.

    private static NeedViewModel ToViewModel(Need need) => new()
    {
        NeedId = need.NeedId,
        NeedTypeId = need.NeedTypeId,
        Title = need.Title,
        Description = need.Description,
        GeometryType = need.GeometryType,
        GeometryData = need.GeometryData,
        ContactPoint = need.ContactPoint,
        NeededFrom = need.NeededFrom,
        Deadline = need.Deadline,
        Quantity = need.Quantity,
        Unit = need.Unit,
        Status = need.Status,
        Priority = need.Priority
    };

    
    /// Kopierer skjemafeltene til behovsmodellen etter validering.
    /// Behovets ID og eier endres ikke av denne metoden.
    private static void Apply(NeedViewModel model, Need need)
    {
        need.NeedTypeId = model.NeedTypeId!.Value;
        need.Title = model.Title;
        need.Description = model.Description;
        need.GeometryType = model.GeometryType!;
        need.GeometryData = model.GeometryData!;
        need.ContactPoint = model.ContactPoint;
        need.NeededFrom = model.NeededFrom;
        need.Deadline = model.Deadline;
        need.Quantity = model.Quantity;
        need.Unit = model.Unit;
        need.Status = model.Status;
        need.Priority = model.Priority;
    }

    /// Kontrollerer om brukeren eier behovet eller har administratorrollen.
    /// Brukes sammen med rollekontrollen på metodene som endrer data.
    private bool CanManage(string ownerId) =>
        ownerId == CurrentUserId || User.IsInRole(AppRoles.Admin);

    
    /// Henter brukerens ID fra identitetsopplysningene som følger innloggingen.
    /// Avbryter dersom en forventet bruker-ID mangler.
    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator.");
}
