using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class MatchController : Controller
{
    private readonly IMatchRepository _matchRepository;
    private readonly INeedRepository _needRepository;
    private readonly IResourceRepository _resourceRepository;

    public MatchController(
        IMatchRepository matchRepository,
        INeedRepository needRepository,
        IResourceRepository resourceRepository)
    {
        _matchRepository = matchRepository;
        _needRepository = needRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<IActionResult> Overview() => View(await _matchRepository.GetActiveAsync());
    public async Task<IActionResult> History() => View(await _matchRepository.GetHistoryAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateMatchViewModel();
        await LoadOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMatchViewModel model)
    {
        var need = await _needRepository.GetByIdAsync(model.NeedId);
        var resource = await _resourceRepository.GetByIdAsync(model.ResourceId);
        if (need is null) ModelState.AddModelError(nameof(model.NeedId), "Behovet finnes ikke.");
        if (resource is null) ModelState.AddModelError(nameof(model.ResourceId), "Ressursen finnes ikke.");
        if (await _matchRepository.ExistsAsync(model.NeedId, model.ResourceId))
            ModelState.AddModelError(string.Empty, "Denne kombinasjonen er allerede matchet.");

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(model);
            return View(model);
        }

        await _matchRepository.CreateAsync(new Match
        {
            NeedId = model.NeedId,
            ResourceId = model.ResourceId,
            Status = MatchStatuses.Proposed,
            MatchedAt = DateTime.UtcNow,
            Notes = model.Notes
        });

        return RedirectToAction(nameof(Overview));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!MatchStatuses.All.Contains(status)) return BadRequest();
        var match = await _matchRepository.GetByIdAsync(id);
        if (match is null) return NotFound();

        var validTransition =
            (match.Status == MatchStatuses.Proposed &&
                (status == MatchStatuses.Approved || status == MatchStatuses.Declined)) ||
            (match.Status == MatchStatuses.Approved && status == MatchStatuses.Completed) ||
            match.Status == status;

        if (!validTransition) return BadRequest("Ugyldig statusovergang.");

        var isClosed = status == MatchStatuses.Completed || status == MatchStatuses.Declined;
        DateTime? resolvedAt = isClosed ? DateTime.UtcNow : null;
        await _matchRepository.UpdateStatusAsync(id, status, resolvedAt);
        return RedirectToAction(isClosed ? nameof(History) : nameof(Overview));
    }

    private async Task LoadOptionsAsync(CreateMatchViewModel model)
    {
        model.Needs = (await _needRepository.GetAllAsync())
            .Select(x => new SelectListItem(x.Title, x.NeedId.ToString()));
        model.Resources = (await _resourceRepository.GetAllAsync())
            .Select(x => new SelectListItem(x.Title, x.ResourceId.ToString()));
    }
}
