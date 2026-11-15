using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

/// Lagrer og henter varsler for innloggede brukere.
public interface INotificationRepository
{
    Task CreateAsync(string userId, string message, string? link);

    Task<IEnumerable<Notification>> GetLatestForUserAsync(string userId, int count);

    Task<int> CountUnreadAsync(string userId);

    /// Markerer ett varsel som lest og returnerer lenken.
    /// Returnerer null dersom varselet ikk tilhører bruker.
    Task<string?> MarkAsReadAsync(int notificationId, string userId);

    Task MarkAllAsReadAsync(string userId);
}