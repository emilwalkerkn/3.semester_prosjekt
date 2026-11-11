using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

public class OrganizationApplicationController : Controller
{
    // Organisasjoner skal kunne åpne søknadssiden uten å være innlogget.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new OrganizationApplicationViewModel());
    }
    
    private readonly ApplicationDbContext _context;

    public OrganizationApplicationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizationApplicationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var organizationNumber = model.OrganizationNumber.Trim();

        // Unngå at samme organisasjon registreres flere ganger.
        var alreadyExists = await _context.Organizations
            .AnyAsync(o => o.OrganizationNumber == organizationNumber);

        if (alreadyExists)
        {
            ModelState.AddModelError(
                nameof(model.OrganizationNumber),
                "En organisasjon med dette organisasjonsnummeret er allerede registrert.");

            return View(model);
        }


        var organization = new Organization
        {
            Name = model.OrganizationName.Trim(),
            OrganizationNumber = organizationNumber,
            Status = "Pending",
            ContactPersonName = model.ContactPersonName.Trim(),
            ContactEmail = model.ContactEmail.Trim(),
            AllowedEmailDomain = model.EmailDomain.Trim().TrimStart('@').ToLowerInvariant()
        };


        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Søknaden er sendt og venter på godkjenning.";

        return RedirectToAction(nameof(Create));
    }
    
}