using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Services;



namespace SemesterProsjekt.Controllers;
//passer på at bare brukere med admin rolle kan se siden
[Authorize(Roles = AppRoles.Admin)]
public class OrganizationApprovalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InvitationEmailSender _emailSender;
    private readonly IConfiguration _configuration;


    public OrganizationApprovalController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        InvitationEmailSender emailSender,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
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

        // Kontroller kontaktpersonens e-postdomene.
        if (string.IsNullOrWhiteSpace(organization.ContactEmail) ||
            string.IsNullOrWhiteSpace(organization.AllowedEmailDomain) ||
            !OrganizationEmailDomainService.IsAllowed(
                organization.ContactEmail,
                organization.AllowedEmailDomain))
        {
            TempData["Error"] =
                "Organisasjonen kan ikke godkjennes fordi kontaktpersonens " +
                "e-postadresse ikke samsvarer med organisasjonens e-postdomene.";

            return RedirectToAction(nameof(Index));
        }
        
        var systemAdmin = await _userManager.GetUserAsync(User);

        if (systemAdmin == null ||
            !await _userManager.IsInRoleAsync(systemAdmin, AppRoles.Admin))
        {
            return Forbid();
        }

        var contactEmail = organization.ContactEmail?.Trim();

        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            TempData["Error"] =
                "Organisasjonen mangler e-postadresse til kontaktpersonen.";

            return RedirectToAction(nameof(Index));
        }

        var existingUser = await _userManager.FindByEmailAsync(contactEmail);

        if (existingUser != null)
        {
            TempData["Error"] =
                "Kontaktpersonen har allerede en brukerkonto. " +
                "Dette må behandles separat.";

            return RedirectToAction(nameof(Index));
        }


        var token = InvitationTokenService.GenerateToken();

        var now = DateTime.UtcNow;

        var invitation = new OrganizationInvitation
        {
            OrganizationId = organization.OrganizationId,
            Email = contactEmail!,
            TokenHash = InvitationTokenService.HashToken(token),
            InvitedByUserId = systemAdmin.Id,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24),
            IsOrganizationAdminInvitation = true
        };

// Organisasjonsgodkjenning og invitasjon lagres i samme databasetransaksjon.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        organization.Status = "Approved";

        _context.OrganizationInvitations.Add(invitation);


        await _context.SaveChangesAsync();

// Bruk den konfigurerte adressen, ikke Host-headeren fra nettleseren.
        var baseUrl = _configuration["Invitation:BaseUrl"];

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsedBaseUrl) ||
            (parsedBaseUrl.Scheme != Uri.UriSchemeHttp &&
             parsedBaseUrl.Scheme != Uri.UriSchemeHttps))
        {
            TempData["Error"] =
                "Invitasjonsadressen er ikke konfigurert riktig.";

            return RedirectToAction(nameof(Index));
        }

        var invitationLink = new Uri(
            parsedBaseUrl,
            $"/OrganizationInvitation/Register?token={Uri.EscapeDataString(token)}"
        ).ToString();

        try
        {
            await _emailSender.SendOrganizationAdminInvitationAsync(
                contactEmail!,
                organization.Name,
                invitationLink);
        }
        catch
        {
            // Transaksjonen blir rullet tilbake når den avslutte uten CommitAsync().
            TempData["Error"] =
                "Organisasjonen kunne ikke godkjennes fordi " +
                "invitasjonen ikke kunne sendes.";

            return RedirectToAction(nameof(Index));
        }

        await transaction.CommitAsync();

        TempData["Success"] =
            "Organisasjonen ble godkjent, og en invitasjon " +
            "ble sendt til kontaktpersonen.";

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
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEmailDomain(
        int organizationId,
        string? allowedEmailDomain)
    {
        var organization = await _context.Organizations
            .FindAsync(organizationId);

        if (organization == null)
            return NotFound();

        // Bare godkjente organisasjoner kan få e-postdomene.
        if (organization.Status != "Approved")
            return Forbid();

        var domain = (allowedEmailDomain ?? "")
            .Trim()
            .TrimStart('@')
            .ToLowerInvariant();

        // Enkelt formatkrav for domenenavn.
        if (domain.Length > 253 ||
            !System.Text.RegularExpressions.Regex.IsMatch(
                domain,
                @"^(?=.{1,253}$)(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,63}$"))
        {
            TempData["Error"] =
                "Oppgi et gyldig e-postdomene, for eksempel politiet.no.";

            return RedirectToAction(nameof(Approved));
        }

        organization.AllowedEmailDomain = domain;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            $"E-postdomenet for {organization.Name} er oppdatert.";

        return RedirectToAction(nameof(Approved));
    }



}