using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

[Authorize]
public class ResourceController : Controller
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceController(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    // Viser en utskriftsvennlig ressursliste.
    public async Task<IActionResult> Print()
    {
        var resources = await _resourceRepository.GetAllAsync();

        return View(resources);
    }

    // Viser alle ressurser, eventuelt filtrert på lokasjon-ID.
    public async Task<IActionResult> Overview()
    {
        var resources = await _resourceRepository.GetAllAsync();

        return View(resources);
    }

    // Viser registreringsskjemaet.
    [HttpGet]
    [Authorize(Roles = "Ressurseier,Admin")]
    public IActionResult Create()
    {
        return View(new ResourceViewModel { UserId = CurrentUserId });
    }

    // Lagrer en ny ressurs.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Ressurseier,Admin")]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resource = new Resource
        {
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
            nameof(Result),
            new { id = resourceId }
        );
    }

    // Viser bekreftelse etter registrering.
    public async Task<IActionResult> Result(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource is null)
        {
            return NotFound();
        }

        return View(resource);
    }

    // Viser detaljer om én ressurs.
    public async Task<IActionResult> Details(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource is null)
        {
            return NotFound();
        }

        ViewData["CurrentUserId"] = CurrentUserId;
        ViewData["CanManageAll"] = CanManageAll;
        return View(resource);
    }

    // Viser skjemaet for å endre en ressurs som tilhører den innloggede brukeren.
    [HttpGet]
    [Authorize(Roles = "Ressurseier,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        if (resource.UserId != CurrentUserId && !CanManageAll) return Forbid();

        return View(ToViewModel(resource));
    }

    // Lagrer endringer på en ressurs. Databasen sjekker også at ressursen eies av brukeren.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Ressurseier,Admin")]
    public async Task<IActionResult> Edit(int id, ResourceViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var resource = ToResource(model);
        resource.ResourceId = id;

        if (!await _resourceRepository.UpdateAsync(resource, CurrentUserId, CanManageAll))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // Sletter en ressurs kun når den innloggede brukeren eier den.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Ressurseier,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _resourceRepository.DeleteAsync(id, CurrentUserId, CanManageAll))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Overview));
    }

    private Resource ToResource(ResourceViewModel model) => new()
    {
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

    private static ResourceViewModel ToViewModel(Resource resource) => new()
    {
        UserId = resource.UserId,
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

    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator."));

    private bool CanManageAll => User.IsInRole("Admin");
}
