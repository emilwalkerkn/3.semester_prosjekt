using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Services;

namespace SemesterProsjekt.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly IAccountRepository _accountRepository;
    private readonly PasswordService _passwordService;

    public SettingsController(
        IAccountRepository accountRepository,
        PasswordService passwordService)
    {
        _accountRepository = accountRepository;
        _passwordService = passwordService;
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

        await _accountRepository.UpdateProfileAsync(CurrentUserId, profile);
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

        var storedHash = await _accountRepository.GetPasswordHashAsync(CurrentUserId);
        if (string.IsNullOrEmpty(storedHash) ||
            !_passwordService.Verify(password.CurrentPassword, storedHash))
        {
            ModelState.AddModelError("Password.CurrentPassword", "Nåværende passord er ikke riktig.");
            return View("Index", await CreatePageModelAsync(password: password));
        }

        await _accountRepository.UpdatePasswordHashAsync(
            CurrentUserId,
            _passwordService.Hash(password.NewPassword));

        TempData["SettingsSuccess"] = "Passordet er oppdatert.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SettingsPageViewModel> CreatePageModelAsync(
        ProfileSettingsViewModel? profile = null,
        PasswordSettingsViewModel? password = null)
    {
        var savedProfile = await _accountRepository.GetProfileAsync(CurrentUserId)
                           ?? new ProfileSettingsViewModel();

        return new SettingsPageViewModel
        {
            Profile = profile ?? savedProfile,
            Password = password ?? new PasswordSettingsViewModel()
        };
    }

    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator."));
}
