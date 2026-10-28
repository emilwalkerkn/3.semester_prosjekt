using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

public class HomeController : Controller
{
    private readonly IResourceRepository _resourceRepository;
    private readonly INeedRepository _needRepository;

    public HomeController(
        IResourceRepository resourceRepository,
        INeedRepository needRepository)
    {
        _resourceRepository = resourceRepository;
        _needRepository = needRepository;
    }

    public async Task<IActionResult> Index()
    {
        var needs = (await _needRepository.GetAllAsync()).ToList();
        var resources = (await _resourceRepository.GetAllAsync()).ToList();

        var model = new DashboardViewModel
        {
            TotalNeeds = needs.Count,
            TotalResources = resources.Count,
            OpenNeeds = needs.Count(n => n.Status != "Resolved"),
            AvailableResources = resources.Count(r => r.Status == "Available"),
            UrgentNeeds = needs.Count(n => n.Status != "Resolved"
                && (n.Priority == "High" || n.Priority == "Critical")),

            RecentNeeds = needs
                .OrderByDescending(n => n.Id)
                .Take(3),

            RecentResources = resources
                .OrderByDescending(r => r.Id)
                .Take(3)
        };

        return View(model);
    }

    public async Task<IActionResult> NeedDetails(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        return View("~/Views/Need/Details.cshtml", need);
    }

    public async Task<IActionResult> ResourceDetails(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource is null)
        {
            return NotFound();
        }

        return View("~/Views/Resource/Details.cshtml", resource);
    }

    // GET: Viser skjemaet for registrering av et behov.
    [HttpGet]
    public IActionResult Need()
    {
        return View("~/Views/Need/Create.cshtml", new NeedViewModel());
    }

    // POST: Registrerer et nytt behov.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Need(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Need/Create.cshtml", model);
        }

        var need = new Need
        {
            Type = model.Type,
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Description = model.Description,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData,
            ContactPoint = model.ContactPoint,
            NeededFrom = model.NeededFrom,
            Deadline = model.Deadline,
            Status = model.Status,
            Priority = model.Priority
        };

        await _needRepository.CreateAsync(need);

        return RedirectToAction("Result", "Need", model);
    }

    // GET: Viser skjemaet for registrering av en ressurs.
    [HttpGet]
    public IActionResult Resource()
    {
        return View("~/Views/Resource/Create.cshtml", new ResourceViewModel());
    }

    // POST: Registrerer en ny ressurs.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resource(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Resource/Create.cshtml", model);
        }

        var resource = new Resource
        {
            Type = model.Type,
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Description = model.Description,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData,
            ContactPoint = model.ContactPoint,
            AvailableFrom = model.AvailableFrom,
            AvailableTo = model.AvailableTo,
            Status = model.Status
        };

        await _resourceRepository.CreateAsync(resource);

        return RedirectToAction("Result", "Resource", model);
    }

    public async Task<IActionResult> ResourceOverview()
    {
        var resources = await _resourceRepository.GetAllAsync();
        return View("~/Views/Resource/Overview.cshtml", resources);
    }

    public async Task<IActionResult> NeedOverview()
    {
        var needs = await _needRepository.GetAllAsync();
        return View("~/Views/Need/Overview.cshtml", needs);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNeedStatus(
        int id,
        string status)
    {
        await _needRepository.UpdateStatusAsync(id, status);

        return RedirectToAction(nameof(NeedOverview));
    }
}