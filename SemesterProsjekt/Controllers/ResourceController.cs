using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
using SemesterProsjekt.Data;

namespace SemesterProsjekt.Controllers;

public class ResourceController : Controller
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceController(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    // Viser en utskriftsvennlig ressursliste, eventuelt filtrert på sted.
    public async Task<IActionResult> Print(string? location = null)
    {
        var resources = await _resourceRepository.GetAllAsync();

        ViewData["LocationFilter"] = location;

        if (!string.IsNullOrWhiteSpace(location))
        {
            resources = resources.Where(resource =>
                resource.Location != null &&
                resource.Location.Contains(
                    location.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            ).ToList();
        }

        return View(resources);
    }
    
    // Viser ressurser og filtrerer på sted hvis et søk er oppgitt.
    public async Task<IActionResult> Overview(string? location = null)
    {
        var resources = await _resourceRepository.GetAllAsync();

        // Beholder søket i søkefeltet.
        ViewData["LocationFilter"] = location;

        if (!string.IsNullOrWhiteSpace(location))
        {
            resources = resources.Where(resource =>
                resource.Location != null &&
                resource.Location.Contains(
                    location.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            ).ToList();
        }

        return View(resources);
    }

    // Viser registreringsskjemaet
    [HttpGet]
    public IActionResult Create()
    {
        var model = new ResourceViewModel();

        return View(model);
    }

    // Lagrer en ny ressurs
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine(
                        $"Valideringsfeil i {entry.Key}: {error.ErrorMessage}"
                    );
                }
            }

            return View(model);
        }

        var resource = new Resource
        {
            Type = model.Type,
            Description = model.Description,
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData,
            ContactPoint = model.ContactPoint,
            AvailableFrom = model.AvailableFrom,
            AvailableTo = model.AvailableTo,
            Status = model.Status
        };

        await _resourceRepository.CreateAsync(resource);

        return RedirectToAction("Result", model);
    }

    // Viser bekreftelse
    public IActionResult Result(ResourceViewModel model)
    {
        return View(model);
    }

    // Viser detaljer om én ressurs
    public async Task<IActionResult> Details(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        return View(resource);
    }
}
