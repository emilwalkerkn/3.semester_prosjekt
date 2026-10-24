using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

[Authorize]
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

            RecentNeeds = needs
                .OrderByDescending(n => n.NeedId)
                .Take(3),

            RecentResources = resources
                .OrderByDescending(r => r.ResourceId)
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

        return View(need);
    }

    public async Task<IActionResult> ResourceDetails(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource is null)
        {
            return NotFound();
        }

        return View(resource);
    }

    // GET: Viser skjemaet for registrering av et behov.
    [HttpGet]
    [Authorize(Roles = "Behovsaktør,Admin")]
    public IActionResult Need()
    {
        return View(new NeedViewModel());
    }

    // POST: Registrerer et nytt behov.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Behovsaktør,Admin")]
    public async Task<IActionResult> Need(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var need = new Need
        {
            UserId = CurrentUserId,
            NeedTypeId = model.NeedTypeId!.Value,
            Title = model.Title,
            Description = model.Description,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData,
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

    // GET: Viser skjemaet for registrering av en ressurs.
    [HttpGet]
    [Authorize(Roles = "Ressurseier,Admin")]
    public IActionResult Resource()
    {
        return View(new ResourceViewModel());
    }

    // POST: Registrerer en ny ressurs.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Ressurseier,Admin")]
    public async Task<IActionResult> Resource(ResourceViewModel model)
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
            nameof(ResourceDetails),
            new { id = resourceId }
        );
    }

    public async Task<IActionResult> ResourceOverview()
    {
        var resources = await _resourceRepository.GetAllAsync();
        return View(resources);
    }

    public async Task<IActionResult> NeedOverview()
    {
        var needs = await _needRepository.GetAllAsync();
        return View(needs);
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
    [Authorize(Roles = "Behovsaktør,Admin")]
    public async Task<IActionResult> UpdateNeedStatus(
        int id,
        string status)
    {
        if (!await _needRepository.UpdateStatusAsync(id, CurrentUserId, User.IsInRole("Admin"), status))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(NeedOverview));
    }

    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator."));
}
