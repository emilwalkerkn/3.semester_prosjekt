using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> Overview(string sortBy = "newest")
    {
        ViewData["SortBy"] = sortBy;
        return View(await _needRepository.GetAllAsync(sortBy));
    }

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new NeedViewModel();
        await LoadNeedTypesAsync(model);
        return View(model);
    }

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeedViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadNeedTypesAsync(model);
            return View(model);
        }

        var need = MapNewNeed(model);
        var id = await _needRepository.CreateAsync(need);
        return RedirectToAction(nameof(Result), new { id });
    }

    public async Task<IActionResult> Result(int id) =>
        await ShowNeedAsync(id, nameof(Result));

    public async Task<IActionResult> Details(int id) =>
        await ShowNeedAsync(id, nameof(Details));

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        if (!CanManage(need.UserId)) return Forbid();

        var model = ToViewModel(need);
        await LoadNeedTypesAsync(model);
        return View(model);
    }

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NeedViewModel model)
    {
        if (id != model.NeedId) return BadRequest();

        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        if (!CanManage(need.UserId)) return Forbid();

        if (!ModelState.IsValid)
        {
            await LoadNeedTypesAsync(model);
            return View(model);
        }

        Apply(model, need);
        await _needRepository.UpdateAsync(need);
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        return CanManage(need.UserId) ? View(need) : Forbid();
    }

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        if (!CanManage(need.UserId)) return Forbid();

        await _needRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Overview));
    }

    [Authorize(Roles = AppRoles.NeedsActor + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!NeedStatuses.All.Contains(status)) return BadRequest();

        var need = await _needRepository.GetByIdAsync(id);
        if (need is null) return NotFound();
        if (!CanManage(need.UserId)) return Forbid();

        await _needRepository.UpdateStatusAsync(id, status);
        return RedirectToAction(nameof(Overview));
    }

    private async Task<IActionResult> ShowNeedAsync(int id, string viewName)
    {
        var need = await _needRepository.GetByIdAsync(id);
        return need is null ? NotFound() : View(viewName, need);
    }

    private async Task LoadNeedTypesAsync(NeedViewModel model)
    {
        model.NeedTypes = (await _needRepository.GetNeedTypesAsync())
            .Select(x => new SelectListItem(x.Name, x.NeedTypeId.ToString()));
    }

    private Need MapNewNeed(NeedViewModel model)
    {
        var need = new Need { UserId = CurrentUserId };
        Apply(model, need);
        return need;
    }

    private static NeedViewModel ToViewModel(Need need) => new()
    {
        NeedId = need.NeedId,
        NeedTypeId = need.NeedTypeId,
        Title = need.Title,
        Description = need.Description,
        GeometryType = need.GeometryType,
        GeometryData = need.GeometryData,
        ContactPoint = need.ContactPoint,
        NeededFrom = need.NeededFrom,
        Deadline = need.Deadline,
        Quantity = need.Quantity,
        Unit = need.Unit,
        Status = need.Status,
        Priority = need.Priority
    };

    private static void Apply(NeedViewModel model, Need need)
    {
        need.NeedTypeId = model.NeedTypeId!.Value;
        need.Title = model.Title;
        need.Description = model.Description;
        need.GeometryType = model.GeometryType!;
        need.GeometryData = model.GeometryData!;
        need.ContactPoint = model.ContactPoint;
        need.NeededFrom = model.NeededFrom;
        need.Deadline = model.Deadline;
        need.Quantity = model.Quantity;
        need.Unit = model.Unit;
        need.Status = model.Status;
        need.Priority = model.Priority;
    }

    private bool CanManage(string ownerId) =>
        ownerId == CurrentUserId || User.IsInRole(AppRoles.Admin);

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator.");
}
