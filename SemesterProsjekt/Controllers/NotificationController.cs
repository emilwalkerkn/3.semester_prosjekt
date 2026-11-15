using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Data;


namespace SemesterProsjekt.Controllers;

/// Håndterer klikk på varsler i toppmeny
[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationRepository _notifications;

    public NotificationController(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    /// Markerer varseelet som lest og sender brukeren til riktig side
    [HttpGet]
    public async Task<IActionResult> Open(int id)
    {
        var link = await _notifications.MarkAsReadAsync(id, CurrentUserId);

        // IsLocalUrl hindrer at en lenke kan sende brukeren til ekstern nettside
        return link is not null && Url.IsLocalUrl(link)
            ? LocalRedirect(link)
            : RedirectToAction("Index", "Home");
    }

    /// Markerer alle varsler som lest og går tilbake til siden brukeren var på
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(string? returnUrl)
    {
        await _notifications.MarkAllAsReadAsync(CurrentUserId);

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl!)
            : RedirectToAction("Index", "Home");
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Innlogget bruker mangler identifikator");
}
