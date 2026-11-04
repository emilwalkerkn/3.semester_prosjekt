using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;


/// Viser innstillings- og personvernsidene for innloggede brukere.
/// Profil og passord oppdateres gjennom ASP.NET Identity.
/// Brukeren hentes fra innloggingen, slik at endringene gjelder egen konto.

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// Mottar Identity-tjenestene gjennom dependency injection.
    /// UserManager håndterer kontoendringer, mens SignInManager
    /// oppdaterer innloggingscookien etter vellykkede endringer.
  
    public SettingsController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// Henter brukerens kontoinformasjon og viser innstillingssiden.
  
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await CreatePageModelAsync());
    }

    /// Viser personvernsiden med samme sidemodell som innstillingssiden.
  
    [HttpGet]
    public async Task<IActionResult> Privacy()
    {
        return View(await CreatePageModelAsync());
    }

    /// Validerer og oppdaterer navn og e-post for den innloggede brukeren.
    /// E-post brukes også som brukernavn ved innlogging.
  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        [Bind(Prefix = "Profile")] ProfileSettingsViewModel profile)
    {
        // Prefikset kobler felter som Profile.FirstName til denne modellen.
        if (!ModelState.IsValid)
        {
            return View("Index", await CreatePageModelAsync(profile: profile));
        }

        // Kontoen hentes fra innloggingen fremfor en bruker-ID fra skjemaet.
        var user = await GetCurrentUserAsync();
        user.FirstName = profile.FirstName.Trim();
        user.LastName = profile.LastName.Trim();
        user.Email = profile.Email.Trim();
        user.UserName = profile.Email.Trim();

        // Bare profilfeltene over oppdateres. Innsendt rolle brukes ikke
        // til å endre brukerens rettigheter.
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View("Index", await CreatePageModelAsync(profile: profile));
        }

        // Fornyer innloggingen slik at den gjenspeiler de lagrede kontoopplysningene.
        await _signInManager.RefreshSignInAsync(user);

        // TempData gjør meldingen tilgjengelig etter videresendingen.
        TempData["SettingsSuccess"] = "Profilen er lagret.";
        return RedirectToAction(nameof(Index));
    }

    /// Endrer passordet for den innloggede brukeren.
    /// Identity kontrollerer nåværende passord og kravene til det nye passordet.
  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePassword(
        [Bind(Prefix = "Password")] PasswordSettingsViewModel password)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await CreatePageModelAsync(password: password));
        }

        var user = await GetCurrentUserAsync();
        var result = await _userManager.ChangePasswordAsync(
            user,
            password.CurrentPassword,
            password.NewPassword);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View("Index", await CreatePageModelAsync(password: password));
        }

        // Oppdaterer den aktive innloggingen etter passordendringen.
        await _signInManager.RefreshSignInAsync(user);

        TempData["SettingsSuccess"] = "Passordet er oppdatert.";
        return RedirectToAction(nameof(Index));
    }

    /// Bygger sidemodellen med profilinformasjon, rolle og passordskjema.
    /// Ved valideringsfeil kan innsendte delmodeller brukes på nytt,
    /// mens de øvrige delene av siden bygges fra brukerens konto.
  
    private async Task<SettingsPageViewModel> CreatePageModelAsync(
        ProfileSettingsViewModel? profile = null,
        PasswordSettingsViewModel? password = null)
    {
        var user = await GetCurrentUserAsync();
        var roles = await _userManager.GetRolesAsync(user);

        return new SettingsPageViewModel
        {
            Profile = profile ?? new ProfileSettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Bruker"
            },
            Password = password ?? new PasswordSettingsViewModel()
        };
    }

    /// Henter Identity-brukeren som tilhører den aktive innloggingen.
    /// Avbryter dersom kontoen ikke finnes.
  
    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User)
               ?? throw new InvalidOperationException(
                   "Den innloggede brukeren finnes ikke.");
    }

    /// Legger feilmeldinger fra Identity i ModelState,
    /// slik at de kan vises i innstillingssidens valideringsoversikt.
  
    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}