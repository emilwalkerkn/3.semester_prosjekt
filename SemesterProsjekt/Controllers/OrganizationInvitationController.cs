using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.ViewModels;
using SemesterProsjekt.Services;

namespace SemesterProsjekt.Controllers;

[Authorize(Roles = AppRoles.OrganizationAdmin)]
public class OrganizationInvitationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InvitationEmailSender _emailSender;
    private readonly IConfiguration _configuration;


    public OrganizationInvitationController(
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
    {
        // Finn administratoren som er logget inn.
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        // Administratoren må tilhøre en organisasjon.
        if (currentUser.OrganizationId == null)
            return Forbid();

        // Organisasjonen må være godkjent.
        var organization = await _context.Organizations
            .FindAsync(currentUser.OrganizationId.Value);

        if (organization == null ||
            organization.Status != "Approved")
            return Forbid();

        // Hent bare invitasjoner fra egen organisasjon.
        var invitations = await _context.OrganizationInvitations
            .Where(i =>
                i.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        ViewBag.OrganizationName = organization.Name;

        return View(invitations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(string email)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser?.OrganizationId == null)
            return Forbid();

        var organization = await _context.Organizations
            .FindAsync(currentUser.OrganizationId.Value);

        if (organization == null || organization.Status != "Approved")
            return Forbid();

        email = email?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email) ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(email))
        {
            TempData["Error"] = "Oppgi en gyldig e-postadresse.";
            return RedirectToAction(nameof(Index));
        }
// Organisasjonen må ha et godkjent e-postdomene.
// Den inviterte må bruke dette domenet.
        if (!OrganizationEmailDomainService.IsAllowed(
                email, organization.AllowedEmailDomain))
        {
            TempData["Error"] =
                "E-postadressen må tilhøre organisasjonens " +
                "godkjente jobbdomene.";

            return RedirectToAction(nameof(Index));
        }
        var normalizedEmail = email.ToUpperInvariant();

        // Kontroller at det ikke allerede finnes en aktiv invitasjon.
        var now = DateTime.UtcNow;

        var existingInvitation =
            await _context.OrganizationInvitations.AnyAsync(i =>
                i.OrganizationId == organization.OrganizationId &&
                i.Email.ToUpper() == normalizedEmail &&
                i.AcceptedAt == null &&
                i.RevokedAt == null &&
                i.ExpiresAt > now);

        if (existingInvitation)
        {
            TempData["Error"] =
                "Det finnes allerede en aktiv invitasjon til denne adressen.";
            return RedirectToAction(nameof(Index));
        }

        var token = InvitationTokenService.GenerateToken();

        var invitation = new OrganizationInvitation
        {
            OrganizationId = organization.OrganizationId,
            Email = email,
            TokenHash = InvitationTokenService.HashToken(token),
            InvitedByUserId = currentUser.Id,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24)
        };


        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();

// Bruk en fast konfigurert adresse, ikke en adresse hentet fra forespørselens Host-header.
        var baseUrl = _configuration["Invitation:BaseUrl"];

        if (!Uri.TryCreate(
                baseUrl,
                UriKind.Absolute,
                out var parsedBaseUrl) ||
            parsedBaseUrl.Scheme != Uri.UriSchemeHttp &&
            parsedBaseUrl.Scheme != Uri.UriSchemeHttps)
        {
            invitation.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return StatusCode(500, "Invitasjonsadresse er ikke konfigurert.");
        }

        var relativePath =
            $"/OrganizationInvitation/Accept?token={Uri.EscapeDataString(token)}";

        var invitationLink = new Uri(
            parsedBaseUrl,
            relativePath).ToString();

        try
        {
            await _emailSender.SendInvitationAsync(
                email,
                organization.Name,
                invitationLink);
        }
        catch
        {
            // Hvis sendingen mislykkes, skal invitasjonen
            // ikke bli stående som aktiv.
            invitation.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Error"] =
                "Invitasjonen kunne ikke sendes. Prøv igjen.";

            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] =
            "Invitasjonen ble opprettet og sendt på e-post.";

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int invitationId)
    {
        // Finn den innloggede administratoren.
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser?.OrganizationId == null)
            return Forbid();

        // Organisasjonen må fortsatt være godkjent.
        var organization = await _context.Organizations
            .FindAsync(currentUser.OrganizationId.Value);

        if (organization == null ||
            organization.Status != "Approved")
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;

        // Kun aktive invitasjoner fra egen organisasjon
        // kan trekkes tilbake.
        var updatedRows = await _context.OrganizationInvitations
            .Where(i =>
                i.OrganizationInvitationId == invitationId &&
                i.OrganizationId == organization.OrganizationId &&
                i.AcceptedAt == null &&
                i.RevokedAt == null &&
                i.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.RevokedAt, now));

        if (updatedRows == 0)
        {
            TempData["Error"] =
                "Invitasjonen finnes ikke, er allerede behandlet " +
                "eller tilhører ikke organisasjonen din.";

            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] =
            "Invitasjonen ble trukket tilbake.";

        return RedirectToAction(nameof(Index));
    }



    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Accept(string? token)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (string.IsNullOrWhiteSpace(token) ||
            token.Length != 64 ||
            !token.All(Uri.IsHexDigit))
        {
            return BadRequest("Ugyldig invitasjonslenke.");
        }

        var tokenHash = InvitationTokenService.HashToken(token);

        var invitation = await _context.OrganizationInvitations
            .Include(i => i.Organization)
            .SingleOrDefaultAsync(i => i.TokenHash == tokenHash);

        if (invitation == null)
            return NotFound("Invitasjonen finnes ikke.");

        if (invitation.AcceptedAt != null ||
            invitation.RevokedAt != null ||
            invitation.ExpiresAt <= DateTime.UtcNow)
        {
            return BadRequest(
                "Invitasjonen er brukt, trukket tilbake eller utløpt.");
        }

        if (invitation.Organization.Status != "Approved")
            return Forbid();
        
        if (!OrganizationEmailDomainService.IsAllowed(
                invitation.Email,
                invitation.Organization.AllowedEmailDomain))
        {
            return BadRequest(
                "Invitasjonens e-postdomene er ikke lenger godkjent.");
        }
        
        ViewBag.OrganizationName = invitation.Organization.Name;

        // En person uten innlogging får se en beskjed.
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction(
                nameof(Register),
                new { token });
        }

        // E-postadressen må være bekreftet.
        if (!await _userManager.IsEmailConfirmedAsync(currentUser))
        {
            ViewBag.InvitationStatus = "EmailNotConfirmed";
            return View();
        }

        // Invitasjonen må være sendt til riktig bruker.
        if (!string.Equals(
                currentUser.Email,
                invitation.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.InvitationStatus = "WrongEmail";
            return View();
        }

        // Brukeren kan ikke bytte organisasjon via invitasjon.
        if (currentUser.OrganizationId != null &&
            currentUser.OrganizationId != invitation.OrganizationId)
        {
            ViewBag.InvitationStatus = "WrongOrganization";
            return View();
        }

        // Brukeren har bestått kontrollene.
        ViewBag.InvitationStatus = "Ready";

        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Register(string? token)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (string.IsNullOrWhiteSpace(token) ||
            token.Length != 64 ||
            !token.All(Uri.IsHexDigit))
        {
            return BadRequest("Ugyldig invitasjonslenke.");
        }

        var tokenHash = InvitationTokenService.HashToken(token);

        var invitation = await _context.OrganizationInvitations
            .Include(i => i.Organization)
            .SingleOrDefaultAsync(i => i.TokenHash == tokenHash);

        if (invitation == null)
            return NotFound("Invitasjonen finnes ikke.");

        if (invitation.AcceptedAt != null ||
            invitation.RevokedAt != null ||
            invitation.ExpiresAt <= DateTime.UtcNow)
        {
            return BadRequest(
                "Invitasjonen er brukt, trukket tilbake eller utløpt.");
        }

        if (invitation.Organization.Status != "Approved")
            return Forbid();
        
        if (!OrganizationEmailDomainService.IsAllowed(
                invitation.Email,
                invitation.Organization.AllowedEmailDomain))
        {
            return BadRequest(
                "Invitasjonens e-postdomene er ikke godkjent.");
        }

        // Registreringen skal foreløpig bare brukes når personen ikke allerede er logget inn.
        if (User.Identity?.IsAuthenticated == true)
        {
            return BadRequest(
                "Du er allerede logget inn. " +
                "Logg ut før du oppretter en ny ansattkonto.");
        }

        // E-post og organisasjon hentes fra databasen.
        ViewBag.InvitedEmail = invitation.Email;
        ViewBag.OrganizationName = invitation.Organization.Name;
        ViewBag.InvitationToken = token;

        return View(
            new SemesterProsjekt.ViewModels
                .InvitedEmployeeRegisterViewModel());
    }
    
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Register(
        string? token,
        InvitedEmployeeRegisterViewModel model)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        
    if (User.Identity?.IsAuthenticated == true)
        return BadRequest("Logg ut før du oppretter en konto.");

    if (string.IsNullOrWhiteSpace(token) ||
        token.Length != 64 ||
        !token.All(Uri.IsHexDigit))
    {
        return BadRequest("Ugyldig invitasjonslenke.");
    }

    var tokenHash = InvitationTokenService.HashToken(token);

    var invitation = await _context.OrganizationInvitations
        .AsNoTracking()
        .Include(i => i.Organization)
        .SingleOrDefaultAsync(i => i.TokenHash == tokenHash);

    if (invitation == null ||
        invitation.AcceptedAt != null ||
        invitation.RevokedAt != null ||
        invitation.ExpiresAt <= DateTime.UtcNow ||
        invitation.Organization.Status != "Approved")
    {
        return BadRequest(
            "Invitasjonen er ugyldig eller ikke lenger aktiv.");
    }
    // Kontroller domenet på nytt ved registrering.
    if (!OrganizationEmailDomainService.IsAllowed(
            invitation.Email,
            invitation.Organization.AllowedEmailDomain))
    {
        return BadRequest(
            "Invitasjonens e-postadresse er ikke lenger " +
            "godkjent for organisasjonen.");
    }

    // Brukes hvis vi må vise registreringsskjemaet igjen.
    ViewBag.InvitedEmail = invitation.Email;
    ViewBag.OrganizationName = invitation.Organization.Name;
    ViewBag.InvitationToken = token;

    if (!ModelState.IsValid)
        return View(model);

    // En eksisterende e-postadresse skal ikke få en ny konto.
    var existingUser =
        await _userManager.FindByEmailAsync(invitation.Email);

    if (existingUser != null)
    {
        ModelState.AddModelError(
            "",
            "Det finnes allerede en konto med denne e-postadressen.");

        return View(model);
    }

    // Identity og invitasjonen bruker samme ApplicationDbContext.
    // Begge endringene skal enten lykkes eller rulles tilbake.
    await using var transaction =
        await _context.Database.BeginTransactionAsync();

    var now = DateTime.UtcNow;

    // Atomisk oppdatering: bare én forespørsel kan bruke tokenet.
    var claimed = await _context.OrganizationInvitations
        .Where(i =>
            i.OrganizationInvitationId ==
                invitation.OrganizationInvitationId &&
            i.TokenHash == tokenHash &&
            i.AcceptedAt == null &&
            i.RevokedAt == null &&
            i.ExpiresAt > now)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(i => i.AcceptedAt, now));

    if (claimed != 1)
    {
        return BadRequest(
            "Invitasjonen er allerede brukt eller utløpt.");
    }
    
// Lås organisasjonen mens registreringen fullføres. Dette hindrer at status eller domene endres samtidig.
    var organizations = await _context.Organizations
        .FromSqlInterpolated(
            $"SELECT * FROM Organizations WHERE OrganizationId = {invitation.OrganizationId} FOR UPDATE")
        .AsNoTracking()
        .ToListAsync();

    var currentOrganization = organizations.SingleOrDefault();

    if (currentOrganization == null ||
        currentOrganization.Status != "Approved" ||
        !OrganizationEmailDomainService.IsAllowed(
            invitation.Email,
            currentOrganization.AllowedEmailDomain))
    {
        return BadRequest(
            "Organisasjonen eller e-postdomenet er ikke lenger godkjent.");
    }
    
    var user = new ApplicationUser
    {
        FirstName = model.FirstName.Trim(),
        LastName = model.LastName.Trim(),

        // Disse verdiene kommer kun fra invitasjonen.
        UserName = invitation.Email,
        Email = invitation.Email,
        OrganizationId = invitation.OrganizationId,

        // Lenken ble levert til den inviterte e-postadressen.
        EmailConfirmed = true
    };

    var createResult =
        await _userManager.CreateAsync(user, model.Password);

    if (!createResult.Succeeded)
    {
        foreach (var error in createResult.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
        // Transaction rulles tilbake når den avsluttes.
    }

    var roleResult =
        await _userManager.AddToRoleAsync(
            user, AppRoles.NeedsActor);

    if (!roleResult.Succeeded)
    {
        foreach (var error in roleResult.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    await transaction.CommitAsync();

    TempData["Success"] =
        "Kontoen ble opprettet. Du kan nå logge inn.";

    return RedirectToAction("Login", "Account");
}




}