using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
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
        var needs = await _needRepository.GetAllAsync();
        var resources = await _resourceRepository.GetAllAsync();

        var model = new DashboardViewModel
        {
            TotalNeeds = needs.Count(),
            TotalResources = resources.Count(),

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

        if (need == null)
        {
            return NotFound();
        }

        return View(need);
    }
    
    
    public async Task<IActionResult> ResourceDetails(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        return View(resource);
    }
    
    
    // GET: Viser skjemaet for registrering av et behov.
    public IActionResult Need()
    {
        var model = new NeedViewModel();
        return View(model);
    }
    
    // POST: Mottar informasjonen fra behovsskjemaet
    // og sender dataene videre til resultatsiden.
    [HttpPost]
    public async Task<IActionResult> Need(NeedViewModel model)
    {
        var need = new Need
        {
            Type = model.Type,
            Description = model.Description,
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData
        };

        await _needRepository.CreateAsync(need);

        return RedirectToAction("NeedResult", model);
    }


    public IActionResult NeedResult(NeedViewModel model)
    {
        return View(model);
    }
    
    // GET: Viser skjemaet for registrering av en ressurs. 
    public IActionResult Resource()
    {
        var model = new ResourceViewModel();
        return View(model);
    }

    // POST: Mottar informasjonen fra ressursskjemaet
    // og sender dataene videre til resultatsiden.
    [HttpPost]
    public async Task<IActionResult> Resource(ResourceViewModel model)
    {
        var resource = new Resource
        {
            Type = model.Type,
            Description = model.Description,
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData
        };

        await _resourceRepository.CreateAsync(resource);

        return RedirectToAction("ResourceResult", model);
    }
    
    public IActionResult ResourceResult(ResourceViewModel model)
    {
        return View(model);
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}