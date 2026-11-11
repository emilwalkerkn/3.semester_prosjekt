using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

//krever OrganizationAdmin-rolle for å få tilgang til denne kontrolleren
[Authorize(Roles = AppRoles.OrganizationAdmin)]
public class OrganizationAdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public OrganizationAdminController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Henter brukeren som er logget inn.
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        // Administrator må tilhøre en organisasjon.
        if (currentUser.OrganizationId == null)
            return Forbid();
        
        //Kontrollerer at organisasjonen faktisk finnes og har status "Approved"
        var organization = await _context.Organizations
            .FindAsync(currentUser.OrganizationId.Value);

        if (organization == null || organization.Status != "Approved")
            return Forbid();

        // Henter bare ansatte fra administratorens organisasjon gjennom asp.net core identity.
        var employees = await _context.Users
            .Where(u => u.OrganizationId == currentUser.OrganizationId)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var employeeRoles = new Dictionary<string, IList<string>>();

        foreach (var employee in employees)
        {
            employeeRoles[employee.Id] =
                await _userManager.GetRolesAsync(employee);
        }

        ViewBag.EmployeeRoles = employeeRoles;

        return View(employees);
        
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveNeedsActorRole(string userId)
    {
        // Finn administratoren som er logget inn.
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        // Administratoren må tilhøre en organisasjon.
        if (currentUser.OrganizationId == null)
            return Forbid();
        

        // Finn brukeren som skal miste rollen.
        var employee = await _userManager.FindByIdAsync(userId);

        if (employee == null)
            return NotFound();

        // Sikkerhetskontroll:
        // Ansatte i andre organisasjoner kan ikke endres selv om korrekt id sendes inn.
        if (employee.OrganizationId != currentUser.OrganizationId)
            return Forbid();
        
        // Kontrollerer at organisasjonen finnes og er godkjent
        var organization = await _context.Organizations
            .FindAsync(currentUser.OrganizationId.Value);

        if (organization == null || organization.Status != "Approved")
            return Forbid();

        // Sjekk at brukeren faktisk har rollen.
        if (!await _userManager.IsInRoleAsync(
                employee, AppRoles.NeedsActor))
        {
            TempData["Error"] = "Brukeren har ikke behovsaktørrollen.";
            return RedirectToAction(nameof(Index));
        }

        // Fjern kun rollen Behovsaktør.
        var result = await _userManager.RemoveFromRoleAsync(
            employee, AppRoles.NeedsActor);

        if (!result.Succeeded)
        {
            TempData["Error"] = "Kunne ikke fjerne rollen.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Behovsaktørrollen ble fjernet.";
        return RedirectToAction(nameof(Index));
    }

}