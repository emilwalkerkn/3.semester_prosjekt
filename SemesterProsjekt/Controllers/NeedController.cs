using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

public class NeedController : Controller
{
    private readonly INeedRepository _needRepository;

    public NeedController(INeedRepository needRepository)
    {
        _needRepository = needRepository;
    }

    // Viser alle behov.
    public async Task<IActionResult> Overview()
    {
        var needs = await _needRepository.GetAllAsync();

        return View(needs);
    }

    // Viser registreringsskjemaet.
    [HttpGet]
    public IActionResult Create()
    {
        return View(new NeedViewModel());
    }

    // Lagrer et nytt behov.
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
            PublicActorId = model.PublicActorId,
            NeedTypeId = model.NeedTypeId,
            LocationId = model.LocationId,
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
            nameof(Result),
            new { id = needId }
        );
    }

    // Viser bekreftelse etter at et behov er registrert.
    public async Task<IActionResult> Result(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        return View(need);
    }

    // Viser full informasjon om ett behov.
    public async Task<IActionResult> Details(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        return View(need);
    }

    // Oppdaterer status på et behov.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _needRepository.UpdateStatusAsync(id, status);

        return RedirectToAction(nameof(Overview));
    }
}