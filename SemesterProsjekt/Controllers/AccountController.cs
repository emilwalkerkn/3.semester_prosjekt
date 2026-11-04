using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;


/// Håndterer registrering, innlogging og utlogging med ASP.NET Identity.
/// UserManager brukes til brukeradministrasjon, mens SignInManager
/// håndterer autentisering og innloggingscookies.

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    
    /// Mottar Identity-tjenestene gjennom dependency injection.
    /// Controlleren bruker disse fremfor å håndtere passord og innlogging selv.
  
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    
    /// Viser registreringsskjemaet for besøkende som ikke er innlogget.
    /// Innloggede brukere videresendes til portalens hjemmeside.
  
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home")!;
        }

        return View(new RegisterViewModel
        {
            ReturnUrl = SafeReturnUrl(returnUrl)
        });
    }

    
    /// Validerer registreringsskjemaet og oppretter en bruker med en tillatt rolle.
    /// Identity kontrollerer passordkrav og lagrer en passordhash.
    /// Ved vellykket registrering logges brukeren inn og videresendes.
  
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Skjulte skjemafelt kan endres av brukeren.
        // Derfor kontrolleres returadressen også ved innsending.
        model.ReturnUrl = SafeReturnUrl(model.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Bare roller som er tillatt ved selvregistrering godtas.
        // Dette hindrer at noen sender inn administratorrollen fra skjemaet.
        if (!AppRoles.SelfServiceRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Ugyldig aktørtype.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

            if (!roleResult.Succeeded)
            {
                // Forsøker å fjerne den nye brukeren dersom rolletildelingen feiler,
                // slik at registreringen ikke etterlater en konto uten valgt rolle.
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            // Oppretter en innloggingscookie uten å velge varig innlogging.
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(model.ReturnUrl);
        }

        // Feil fra Identity sendes tilbake til registreringsskjemaet.
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    
    /// Viser innloggingsskjemaet med en kontrollert returadresse.
    /// Brukere som allerede er innlogget, sendes til hjemmesiden.
  
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home")!;
        }

        return View(new LoginViewModel
        {
            ReturnUrl = SafeReturnUrl(returnUrl)
        });
    }

    
    /// Validerer skjemaet og forsøker innlogging med Identity.
    /// Feil passord teller mot kontoens låsegrense, konfigurert i Program.cs.
    /// Ved vellykket innlogging sendes brukeren til en kontrollert lokal adresse.
  
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Returadressen kontrolleres på nytt fordi den kommer fra skjemaet.
        model.ReturnUrl = SafeReturnUrl(model.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Kontoen er midlertidig låst. Prøv igjen om 15 minutter.");
        }
        else
        {
            // Samme feilmelding brukes ved feil e-post og feil passord,
            // slik at meldingen ikke avslører om kontoen finnes.
            ModelState.AddModelError(
                string.Empty,
                "E-post eller passord er feil.");
        }

        return View(model);
    }

    
    /// Logger brukeren ut ved å fjerne innloggingscookien.
    /// Videresender til Home/Index, der tilgangskontrollen krever ny innlogging.
    /// Utlogging krever POST og et gyldig sikkerhetstoken.
  
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home")!;
    }

    
    /// Viser informasjon om manglende tilgang.
    /// Siden kan åpnes uten innlogging.
  
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    
    /// Videresender til en godkjent lokal returadresse.
    /// Bruker hjemmesiden dersom returadressen mangler eller blir avvist.
  
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return SafeReturnUrl(returnUrl) is { } localUrl
            ? LocalRedirect(localUrl)
            : RedirectToAction(nameof(HomeController.Index), "Home")!;
    }

    
    /// Avviser tomme og eksterne returadresser, samt adresser til
    /// kontosidene og feilsiden. Dette beskytter mot ekstern videresending
    /// og begrenser videresendingsløkker mellom disse sidene.
  
    private string? SafeReturnUrl(string? returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl) ||
            string.IsNullOrWhiteSpace(returnUrl) ||
            returnUrl.StartsWith("/Account/", StringComparison.OrdinalIgnoreCase) ||
            returnUrl.StartsWith("/Home/Error", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return returnUrl;
    }
}
