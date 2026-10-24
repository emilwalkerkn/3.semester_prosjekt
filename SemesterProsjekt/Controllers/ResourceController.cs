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
    public IActionResult Create()
    {
        return View(new ResourceViewModel { UserId = CurrentUserId });
    }

    // Lagrer en ny ressurs.
    [HttpPost]
    [ValidateAntiForgeryToken]
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

        return View(resource);
    }

    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator."));
}
