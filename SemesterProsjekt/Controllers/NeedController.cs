using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
using SemesterProsjekt.Data;

namespace SemesterProsjekt.Controllers;

public class NeedController : Controller
{
    private readonly INeedRepository _needRepository;

    public NeedController(INeedRepository needRepository)
    {
        _needRepository = needRepository;
    }

    // Viser alle behov
    public async Task<IActionResult> Overview()
    {
        var needs = await _needRepository.GetAllAsync();
        return View(needs);
    }

    // Viser registreringsskjemaet
    [HttpGet]
    public IActionResult Create()
    {
        var model = new NeedViewModel();
        return View(model);
    }

    // Lagrer et nytt behov
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var need = new Need
        {
            Type = model.Type,
            Description = model.Description,
            Location = model.Location,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            GeometryType = model.GeometryType,
            GeometryData = model.GeometryData,
            Status = model.Status,
            Priority = model.Priority,
            ContactPoint = model.ContactPoint,
            NeededFrom = model.NeededFrom,
            Deadline = model.Deadline
        };

        await _needRepository.CreateAsync(need);

        return RedirectToAction("Result", model);
    }

    // Viser bekreftelse
    public IActionResult Result(NeedViewModel model)
    {
        return View(model);
    }

    // Viser detaljer om ett behov
    public async Task<IActionResult> Details(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need == null)
        {
            return NotFound();
        }

        return View(need);
    }

    // Oppdaterer status på et behov
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _needRepository.UpdateStatusAsync(id, status);

        return RedirectToAction("Overview");
    }
}
