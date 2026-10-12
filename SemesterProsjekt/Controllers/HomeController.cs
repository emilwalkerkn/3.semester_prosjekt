using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
using SemesterProsjekt.Data;

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

    // Viser forsiden med statistikk og siste registreringer
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
}