using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

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

        ViewData["CurrentUserId"] = CurrentUserId;
        ViewData["CanManageAll"] = CanManageAll;
        return View(needs);
    }

    // Viser registreringsskjemaet.
    [HttpGet]
    [Authorize(Roles = "Behovsaktør,Admin")]
    public async Task<IActionResult> Create()
    {
        var model = new NeedViewModel { UserId = CurrentUserId };
        await LoadNeedTypesAsync(model);
        return View(model);
    }

    // Lagrer et nytt behov.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Behovsaktør,Admin")]
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

        ViewData["CurrentUserId"] = CurrentUserId;
        ViewData["CanManageAll"] = CanManageAll;
        return View(need);
    }

    // Oppdaterer status på et behov.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Behovsaktør,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!await _needRepository.UpdateStatusAsync(id, CurrentUserId, CanManageAll, status))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Overview));
    }

    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator."));

    private bool CanManageAll => User.IsInRole("Admin");
}
