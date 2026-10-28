using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SemesterProsjekt.Controllers;

[Authorize]
public class NeedController : Controller
{
    private readonly INeedRepository _needRepository;

    public NeedController(INeedRepository needRepository)
    {
        _needRepository = needRepository;
    }

    private async Task LoadNeedTypesAsync(NeedViewModel model)
    {
        var needTypes = await _needRepository.GetNeedTypesAsync();
        model.NeedTypes = needTypes.Select(needType => new SelectListItem
        {
            Value = needType.NeedTypeId.ToString(),
            Text = needType.Name
        });
    }

    // Viser alle behov.
    public async Task<IActionResult> Overview()
    {
        var needs = await _needRepository.GetAllAsync();

        return View(needs);
    }

    // Viser registreringsskjemaet.
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new NeedViewModel();
        await LoadNeedTypesAsync(model);
        return View(model);
    }

    // Lagrer et nytt behov.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadNeedTypesAsync(model);
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
    
    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "Innlogget bruker mangler identifikator."));
    
}