using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SemesterProsjekt.Controllers;


/// Viser portalens hjemmeside med antall og utvalgte registreringer.
/// Inneholder også handlinger for registrering, visning og statusoppdatering.
/// Innlogging kreves for alle handlinger unntatt feilsiden.

[Authorize]
public class HomeController : Controller
{
    private readonly IResourceRepository _resourceRepository;
    private readonly INeedRepository _needRepository;

  
    /// Mottar repositoryene gjennom dependency injection.
    /// De brukes til å hente og lagre behov og ressurser uten
    /// at controlleren håndterer SQL eller databasetilkoblinger.
   
    public HomeController(
        IResourceRepository resourceRepository,
        INeedRepository needRepository)
    {
        _resourceRepository = resourceRepository;
        _needRepository = needRepository;
    }

  
    /// Bygger hjemmesidens oversikt med totalt antall behov og ressurser.
    /// De tre registreringene med høyest ID fra hver kategori vises også.
   
    public async Task<IActionResult> Index()
    {
        var needs = (await _needRepository.GetAllAsync()).ToList();
        var resources = (await _resourceRepository.GetAllAsync()).ToList();

        var model = new DashboardViewModel
        {
            TotalNeeds = needs.Count,
            TotalResources = resources.Count,

            RecentNeeds = needs
                .OrderByDescending(n => n.NeedId)
                .Take(3),

            RecentResources = resources
                .OrderByDescending(r => r.ResourceId)
                .Take(3)
        };

        return View(model);
    }
    
    
    /// Henter og viser ett behov.
    /// Returnerer HTTP 404 dersom behovet ikke finnes.
   
    public async Task<IActionResult> NeedDetails(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        return View(need);
    }

  
    /// Henter og viser én ressurs.
    /// Returnerer HTTP 404 dersom ressursen ikke finnes.
   
    public async Task<IActionResult> ResourceDetails(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource is null)
        {
            return NotFound();
        }

        return View(resource);
    }

  
    /// Viser et tomt behovsskjema for behovsaktører og administratorer.
   
    [HttpGet]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    public IActionResult Need()
    {
        return View(new NeedViewModel());
    }

  
    /// Validerer skjemamodellen og lagrer et behov
    /// knyttet til den innloggede brukeren.
    /// Ved vellykket lagring vises behovets detaljer.
   
    [HttpPost]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Need(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var need = new Need
        {
            // Eierens ID hentes fra innloggingen fremfor skjemaet.
            UserId = CurrentUserId,
            NeedTypeId = model.NeedTypeId!.Value,
            Title = model.Title,
            Description = model.Description,
            GeometryType = model.GeometryType!,
            GeometryData = model.GeometryData!,
            ContactPoint = model.ContactPoint,
            NeededFrom = model.NeededFrom,
            Deadline = model.Deadline,
            Quantity = model.Quantity,
            Unit = model.Unit,
            Status = model.Status,
            Priority = model.Priority
        };

        var needId = await _needRepository.CreateAsync(need);

        return RedirectToAction(
            nameof(NeedDetails),
            new { id = needId }
        );
    }

  
    /// Viser et tomt ressursskjema for ressurseiere og administratorer.
   
    [HttpGet]
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    public IActionResult Resource()
    {
        return View(new ResourceViewModel());
    }

  
    /// Validerer skjemamodellen og lagrer en ressurs
    /// knyttet til den innloggede brukeren.
    /// Ved vellykket lagring vises ressursens detaljer.
   
    [HttpPost]
    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resource(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resource = new Resource
        {
            // Brukeren kan ikke velge en annen eier gjennom skjemaet.
            UserId = CurrentUserId,
            ResourceTypeId = model.ResourceTypeId,
            Title = model.Title,
            Description = model.Description,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData,
            ContactPoint = model.ContactPoint,
            AvailableFrom = model.AvailableFrom,
            AvailableTo = model.AvailableTo,
            Quantity = model.Quantity,
            Unit = model.Unit,
            Status = model.Status
        };

        var resourceId = await _resourceRepository.CreateAsync(resource);

        return RedirectToAction(
            nameof(ResourceDetails),
            new { id = resourceId }
        );
    }

  
    /// Henter alle ressurser og sender dem til ressursoversikten.
   
    public async Task<IActionResult> ResourceOverview()
    {
        var resources = await _resourceRepository.GetAllAsync();
        return View(resources);
    }

  
    /// Henter alle behov og sender dem til behovsoversikten.
   
    public async Task<IActionResult> NeedOverview()
    {
        var needs = await _needRepository.GetAllAsync();
        return View(needs);
    }

  
    /// Viser personvernsiden som tilhører HomeController.
   
    public IActionResult Privacy()
    {
        return View();
    }

  
    /// Viser feilsiden med en forespørsels-ID som kan brukes
    /// til å finne den tilhørende hendelsen i loggene.
    /// Siden tillater anonyme besøk og skal ikke mellomlagres.
   
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
    }

  
    /// Oppdaterer et behov til en tillatt status.
    /// Brukeren må være behovsaktør eller administrator,
    /// og må eie behovet dersom vedkommende ikke er administrator.
   
    [HttpPost]
    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNeedStatus(
        int id,
        string status)
    {
        if (!NeedStatuses.All.Contains(status))
        {
            return BadRequest();
        }

        var need = await _needRepository.GetByIdAsync(id);
        if (need is null)
        {
            return NotFound();
        }

        // Riktig rolle alene gir ikke tilgang til å endre andres behov.
        if (need.UserId != CurrentUserId && !User.IsInRole(AppRoles.Admin))
        {
            return Forbid();
        }

        await _needRepository.UpdateStatusAsync(id, status);

        return RedirectToAction(nameof(NeedOverview));
    }

  
    /// Henter brukerens ID fra identitetsopplysningene som følger innloggingen.
    /// Avbryter dersom en forventet bruker-ID mangler.
   
    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "Innlogget bruker mangler identifikator.");
}