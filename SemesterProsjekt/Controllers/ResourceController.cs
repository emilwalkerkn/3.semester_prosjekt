using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;


/// Håndterer visning og behandling av ressurser for innloggede brukere.
/// Databasetilgang skjer gjennom IResourceRepository.
/// Endringer krever riktig rolle og at brukeren eier ressursen eller er administrator.

[Authorize]
public class ResourceController : Controller
{
    private readonly IResourceRepository _resourceRepository;

  
    /// Mottar repositoryet gjennom dependency injection, slik at controlleren
    /// kan hente og lagre ressurser uten å håndtere SQL eller databasetilkoblinger.
    public ResourceController(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

  
    /// Henter alle ressurser og viser dem i en utskriftsvennlig visning.
    public async Task<IActionResult> Print() =>
        View(await _resourceRepository.GetAllAsync());

  
    /// Henter alle ressurser og sender dem til ressursoversikten.
    public async Task<IActionResult> Overview() =>
        View(await _resourceRepository.GetAllAsync());

  
    /// Viser et tomt registreringsskjema med ressurstyper hentet fra databasen.
    /// Bare ressurseiere og administratorer kan registrere ressurser.
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ResourceViewModel();
        await LoadResourceTypesAsync(model);
        return View(model);
    }

  
    /// Validerer skjemaet og tilgjengelighetsdatoene før en ny ressurs lagres.
    /// Ressursen knyttes til den innloggede brukeren, som deretter
    /// videresendes til bekreftelsessiden.
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {
        ValidateDates(model);

        if (!ModelState.IsValid)
        {
            // Nedtrekksvalgene må lastes på nytt når skjemaet vises med feil.
            await LoadResourceTypesAsync(model);
            return View(model);
        }

        var resource = MapNewResource(model);
        var id = await _resourceRepository.CreateAsync(resource);
        return RedirectToAction(nameof(Result), new { id });
    }

  
    /// Viser bekreftelsen for en registrert ressurs.
    public async Task<IActionResult> Result(int id) =>
        await ShowResourceAsync(id, nameof(Result));

  
    /// Viser detaljene for en ressurs.
    public async Task<IActionResult> Details(int id) =>
        await ShowResourceAsync(id, nameof(Details));

  
    /// Henter en eksisterende ressurs og viser redigeringsskjemaet.
    /// Bare eieren eller en administrator med riktig rolle får tilgang.
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        if (!CanManage(resource.UserId)) return Forbid();

        var model = ToViewModel(resource);
        await LoadResourceTypesAsync(model);
        return View(model);
    }

  
    /// Kontrollerer ressursens ID, brukerens tilgang og skjemadata
    /// før endringer lagres på den eksisterende ressursen.
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResourceViewModel model)
    {
        // Adressen og skjemaet må vise til samme ressurs.
        if (id != model.ResourceId) return BadRequest();

        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();

        // Tilgangen kontrolleres også ved innsending, ikke bare når skjemaet åpnes.
        if (!CanManage(resource.UserId)) return Forbid();

        ValidateDates(model);

        if (!ModelState.IsValid)
        {
            await LoadResourceTypesAsync(model);
            return View(model);
        }

        Apply(model, resource);
        await _resourceRepository.UpdateAsync(resource);
        return RedirectToAction(nameof(Details), new { id });
    }

  
    /// Viser en bekreftelsesside før sletting.
    /// Denne GET-forespørselen sletter ikke data.
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        return CanManage(resource.UserId) ? View(resource) : Forbid();
    }

  
    /// Sletter ressursen etter bekreftelse og ny kontroll av brukerens tilgang.
    /// Metoden mottar POST-forespørsler til Delete.
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        if (!CanManage(resource.UserId)) return Forbid();

        await _resourceRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Overview));
    }

  
    /// Felles oppslag for detalj- og bekreftelsessidene.
    /// Returnerer HTTP 404 dersom ressursen ikke finnes.
    private async Task<IActionResult> ShowResourceAsync(int id, string viewName)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        return resource is null ? NotFound() : View(viewName, resource);
    }

  
    /// Gjør ressurstypene fra databasen om til valg i skjemaets nedtrekksliste.
    /// Typens navn vises til brukeren, mens ID-en sendes inn med skjemaet.
    private async Task LoadResourceTypesAsync(ResourceViewModel model)
    {
        model.ResourceTypes = (await _resourceRepository.GetResourceTypesAsync())
            .Select(x => new SelectListItem(x.Name, x.ResourceTypeId.ToString()));
    }

  
    /// Kontrollerer at sluttdatoen ikke er før startdatoen når begge er oppgitt.
    /// Eventuelle feil legges i ModelState og vises ved sluttdatoen i skjemaet.
    private void ValidateDates(ResourceViewModel model)
    {
        if (model.AvailableFrom.HasValue && model.AvailableTo.HasValue &&
            model.AvailableTo < model.AvailableFrom)
        {
            ModelState.AddModelError(
                nameof(model.AvailableTo),
                "Tilgjengelig til kan ikke være før tilgjengelig fra.");
        }
    }

  
    /// Oppretter en ressursmodell fra skjemaet og setter den innloggede brukeren som eier.
    /// Eierens ID hentes fra innloggingen, slik at den ikke kan velges i skjemaet.
    private Resource MapNewResource(ResourceViewModel model)
    {
        var resource = new Resource { UserId = CurrentUserId };
        Apply(model, resource);
        return resource;
    }

  
    /// Overfører en lagret ressurs til modellen som brukes i redigeringsskjemaet.
    private static ResourceViewModel ToViewModel(Resource resource) => new()
    {
        ResourceId = resource.ResourceId,
        ResourceTypeId = resource.ResourceTypeId,
        Title = resource.Title,
        Description = resource.Description,
        GeometryType = resource.GeometryType,
        GeometryData = resource.GeometryData,
        ContactPoint = resource.ContactPoint,
        AvailableFrom = resource.AvailableFrom,
        AvailableTo = resource.AvailableTo,
        Quantity = resource.Quantity,
        Unit = resource.Unit,
        Status = resource.Status
    };

  
    /// Kopierer skjemafeltene til ressursmodellen etter validering.
    /// Ressursens ID og eier endres ikke av denne metoden.
    private static void Apply(ResourceViewModel model, Resource resource)
    {
        resource.ResourceTypeId = model.ResourceTypeId;
        resource.Title = model.Title;
        resource.Description = model.Description;
        resource.GeometryType = model.GeometryType;
        resource.GeometryData = model.GeometryData;
        resource.ContactPoint = model.ContactPoint;
        resource.AvailableFrom = model.AvailableFrom;
        resource.AvailableTo = model.AvailableTo;
        resource.Quantity = model.Quantity;
        resource.Unit = model.Unit;
        resource.Status = model.Status;
    }

  
    /// Kontrollerer om brukeren eier ressursen eller har administratorrollen.
    /// Brukes sammen med rollekontrollen på metodene som endrer data.
    private bool CanManage(string ownerId) =>
        ownerId == CurrentUserId || User.IsInRole(AppRoles.Admin);

  
    /// Henter brukerens ID fra identitetsopplysningene som følger innloggingen.
    /// Avbryter dersom en forventet bruker-ID mangler.
    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator.");
}