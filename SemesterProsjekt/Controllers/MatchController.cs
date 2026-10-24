using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;

[Authorize]
public class MatchController : Controller
{
    private readonly IMatchRepository _matchRepository;
    private readonly INeedRepository _needRepository;

    public MatchController(IMatchRepository matchRepository, INeedRepository needRepository)
    {
        _matchRepository = matchRepository;
        _needRepository = needRepository;
    }

    // Viser bare matcher brukeren selv er part i, eller alle matcher for administrator.
    public async Task<IActionResult> Overview()
    {
        var matches = await _matchRepository.GetVisibleAsync(CurrentUserId, CanManageAll);
        return View(new MatchOverviewViewModel
        {
            Matches = matches,
            CurrentUserId = CurrentUserId,
            CanManageAll = CanManageAll
        });
    }

    [HttpGet]
    [Authorize(Roles = "Behovsaktør,Admin")]
    public async Task<IActionResult> Create(int needId)
    {
        var need = await _needRepository.GetByIdAsync(needId);
        if (need is null) return NotFound();
        if (need.UserId != CurrentUserId && !CanManageAll) return Forbid();

        return View(new MatchCreateViewModel
        {
            Need = need,
            AvailableResources = await _matchRepository.GetAvailableResourcesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Behovsaktør,Admin")]
    public async Task<IActionResult> Create(int needId, MatchCreateViewModel model)
    {
        var need = await _needRepository.GetByIdAsync(needId);
        if (need is null) return NotFound();
        if (need.UserId != CurrentUserId && !CanManageAll) return Forbid();

        if (!ModelState.IsValid)
        {
            model.Need = need;
            model.AvailableResources = await _matchRepository.GetAvailableResourcesAsync();
            return View(model);
        }

        var matchId = await _matchRepository.CreateAsync(
            needId,
            model.ResourceId!.Value,
            model.AssignedQuantity,
            CurrentUserId,
            CanManageAll);

        if (matchId is null)
        {
            ModelState.AddModelError(string.Empty,
                "Matchen kunne ikke opprettes. Kontroller at ressursen er ledig og at mengden ikke overskrider behovet eller ressursen.");
            model.Need = need;
            model.AvailableResources = await _matchRepository.GetAvailableResourcesAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Overview));
    }

    // Ressurseier kan godta eller avslå et forslag som gjelder egen ressurs.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Ressurseier,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!await _matchRepository.UpdateStatusAsync(id, CurrentUserId, CanManageAll, status))
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
