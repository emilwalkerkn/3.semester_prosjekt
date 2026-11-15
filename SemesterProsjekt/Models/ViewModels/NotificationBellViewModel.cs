namespace SemesterProsjekt.Models.ViewModels;

/// Data til bjella i toppmeny
public class NotificationBellViewModel
{
    public List<Notification> Items { get; set; } = [];
    public int UnreadCount { get; set; }

    // Gjør et UTC-tidspunkt om til kort tekst, som "5 min sida"
    public static string TimeAgo(DateTime createdAtUtc)
    {
        var age = DateTime.UtcNow - createdAtUtc;
        if (age.TotalMinutes < 1) return "Akkurat nå";
        if (age.TotalHours < 1) return $"{(int)age.TotalMinutes} min siden";
        if (age.TotalDays < 1) return $"{(int)age.TotalHours} t siden";
        return createdAtUtc.ToString("dd.MM.yyyy");
    }
}