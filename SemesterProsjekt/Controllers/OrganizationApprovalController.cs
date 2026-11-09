using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Microsoft.AspNetCore.Identity;

namespace SemesterProsjekt.Controllers;
//passer på at bare brukere med admin rolle kan se siden
[Authorize(Roles = AppRoles.Admin)]
public class OrganizationApprovalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrganizationApprovalController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {   // Henter alle organisasjoner med status "Pending" fra databasen, sortert etter opprettelsesdato.
        var pendingOrganizations = await _context.Organizations
            .Where(o => o.Status == "Pending")
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        return View(pendingOrganizations);
    }
    
    [HttpGet]
    public async Task<IActionResult> Approved()
    {
        var organizations = await _context.Organizations
            .Where(o => o.Status == "Approved")
            .OrderBy(o => o.Name)
            .ToListAsync();

        return View(organizations);
    }

    //post for å forhinde uønskede forespørsler
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int organizationId)
    {
        var organization = await _context.Organizations
            .FindAsync(organizationId);

        if (organization == null)
            return NotFound();

        // Bare organisasjoner som venter, kan godkjennes.
        if (organization.Status != "Pending")
            return Conflict("Organisasjonen er allerede behandlet.");

        organization.Status = "Approved";

        await _context.SaveChangesAsync();

        TempData["Success"] = "Organisasjonen ble godkjent.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int organizationId)
    {
        var organization = await _context.Organizations
            .FindAsync(organizationId);

        if (organization == null)
            return NotFound();

        // Bare organisasjoner som venter, kan avslås.
        if (organization.Status != "Pending")
            return Conflict("Organisasjonen er allerede behandlet.");

        organization.Status = "Rejected";

        await _context.SaveChangesAsync();

        TempData["Success"] = "Organisasjonen ble avslått.";

        return RedirectToAction(nameof(Index));
        
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignOrganizationAdmin(
        int organizationId,
        string email)
    {
        var organization = await _context.Organizations
            .FindAsync(organizationId);

        if (organization == null)
            return NotFound();

        // Bare godkjente organisasjoner kan få administrator.
        if (organization.Status != "Approved")
            return Forbid();

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            TempData["Error"] = "Brukeren finnes ikke.";
            return RedirectToAction(nameof(Index));
        }

        // Brukeren må allerede tilhøre organisasjonen.
        if (user.OrganizationId != organizationId)
            return Forbid();
        
        // En systemadministrator skal ikke utpekes som organisasjonsadministrator.
        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            TempData["Error"] =
                "En systemadministrator kan ikke utpekes som organisasjonsadministrator.";

            return RedirectToAction(nameof(Approved));
        }

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return StatusCode(500);

        if (!await _userManager.IsInRoleAsync(
                user, AppRoles.OrganizationAdmin))
        {
            var roleResult = await _userManager.AddToRoleAsync(
                user, AppRoles.OrganizationAdmin);

            if (!roleResult.Succeeded)
                return StatusCode(500);
        }

        TempData["Success"] = "Organisasjonsadministrator ble tildelt.";
        return RedirectToAction(nameof(Approved));
    }


}