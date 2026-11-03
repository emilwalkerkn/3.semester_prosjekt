using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Controllers;

/// <summary>
/// Lar en innlogget bruker administrere konto og lokale portalinnstillinger.
/// </summary>
[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SettingsController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await CreatePageModelAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Privacy()
    {
        return View(await CreatePageModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        [Bind(Prefix = "Profile")] ProfileSettingsViewModel profile)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await CreatePageModelAsync(profile: profile));
        }

        var user = await GetCurrentUserAsync();
        user.FirstName = profile.FirstName.Trim();
        user.LastName = profile.LastName.Trim();
        user.Email = profile.Email.Trim();
        user.UserName = profile.Email.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View("Index", await CreatePageModelAsync(profile: profile));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SettingsSuccess"] = "Profilen er lagret.";
        return RedirectToAction(nameof(Index));
    }

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

        await _signInManager.RefreshSignInAsync(user);
        TempData["SettingsSuccess"] = "Passordet er oppdatert.";
        return RedirectToAction(nameof(Index));
    }

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

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User)
               ?? throw new InvalidOperationException("Den innloggede brukeren finnes ikke.");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
