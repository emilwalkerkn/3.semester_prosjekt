using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.ViewComponents;

/// En "Mini-controller" for layouten: henter varslene til den innloggede
/// brukeren slik at klokken kan vise på alle sider.

public class NotificationBellViewComponent : ViewComponent
{
    private readonly INotificationRepository _notifications;

    public NotificationBellViewComponent(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Content(string.Empty);

        return View(new NotificationBellViewModel
        {
            Items = (await _notifications.GetLatestForUserAsync(userId, 8)).ToList(),
            UnreadCount = await _notifications.CountUnreadAsync(userId)
        });
    }
}