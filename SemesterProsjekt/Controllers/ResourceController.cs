using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

[Authorize]
public class ResourceController : Controller
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceController(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<IActionResult> Print() => View(await _resourceRepository.GetAllAsync());
    public async Task<IActionResult> Overview() => View(await _resourceRepository.GetAllAsync());

    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ResourceViewModel();
        await LoadResourceTypesAsync(model);
        return View(model);
    }

    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {
        ValidateDates(model);
        if (!ModelState.IsValid)
        {
            await LoadResourceTypesAsync(model);
            return View(model);
        }

        var resource = MapNewResource(model);
        var id = await _resourceRepository.CreateAsync(resource);
        return RedirectToAction(nameof(Result), new { id });
    }

    public async Task<IActionResult> Result(int id) => await ShowResourceAsync(id, nameof(Result));
    public async Task<IActionResult> Details(int id) => await ShowResourceAsync(id, nameof(Details));

    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        if (!CanManage(resource.UserId)) return Forbid();

        var model = ToViewModel(resource);
        await LoadResourceTypesAsync(model);
        return View(model);
    }

    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResourceViewModel model)
    {
        if (id != model.ResourceId) return BadRequest();
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        if (!CanManage(resource.UserId)) return Forbid();

        ValidateDates(model);
        if (!ModelState.IsValid)
        {
            await LoadResourceTypesAsync(model);
            return View(model);
        }

        Apply(model, resource);
        await _resourceRepository.UpdateAsync(resource);
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        return CanManage(resource.UserId) ? View(resource) : Forbid();
    }

    [Authorize(Roles = AppRoles.ResourceOwner + "," + AppRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource is null) return NotFound();
        if (!CanManage(resource.UserId)) return Forbid();

        await _resourceRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Overview));
    }

    private async Task<IActionResult> ShowResourceAsync(int id, string viewName)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        return resource is null ? NotFound() : View(viewName, resource);
    }

    private async Task LoadResourceTypesAsync(ResourceViewModel model)
    {
        model.ResourceTypes = (await _resourceRepository.GetResourceTypesAsync())
            .Select(x => new SelectListItem(x.Name, x.ResourceTypeId.ToString()));
    }

    private void ValidateDates(ResourceViewModel model)
    {
        if (model.AvailableFrom.HasValue && model.AvailableTo.HasValue &&
            model.AvailableTo < model.AvailableFrom)
        {
            ModelState.AddModelError(
                nameof(model.AvailableTo),
                "Tilgjengelig til kan ikke være før tilgjengelig fra.");
        }
    }

    private Resource MapNewResource(ResourceViewModel model)
    {
        var resource = new Resource { UserId = CurrentUserId };
        Apply(model, resource);
        return resource;
    }

    private static ResourceViewModel ToViewModel(Resource resource) => new()
    {
        ResourceId = resource.ResourceId,
        ResourceTypeId = resource.ResourceTypeId,
        Title = resource.Title,
        Description = resource.Description,
        GeometryType = resource.GeometryType,
        GeometryData = resource.GeometryData,
        ContactPoint = resource.ContactPoint,
        AvailableFrom = resource.AvailableFrom,
        AvailableTo = resource.AvailableTo,
        Quantity = resource.Quantity,
        Unit = resource.Unit,
        Status = resource.Status
    };

    private static void Apply(ResourceViewModel model, Resource resource)
    {
        resource.ResourceTypeId = model.ResourceTypeId;
        resource.Title = model.Title;
        resource.Description = model.Description;
        resource.GeometryType = model.GeometryType;
        resource.GeometryData = model.GeometryData;
        resource.ContactPoint = model.ContactPoint;
        resource.AvailableFrom = model.AvailableFrom;
        resource.AvailableTo = model.AvailableTo;
        resource.Quantity = model.Quantity;
        resource.Unit = model.Unit;
        resource.Status = model.Status;
    }

    private bool CanManage(string ownerId) =>
        ownerId == CurrentUserId || User.IsInRole(AppRoles.Admin);

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator.");
}
