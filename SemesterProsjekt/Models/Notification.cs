namespace SemesterProsjekt.Models;

/// Et varsel til en bruker. Speiler Notifications-tabellen i databse
public class Notification
{
    public int NotificationId { get; set; }

    // Brukeren som skal se varselet (Id fra AspNetUsers).
    public string UserId { get; set; } = "";

    public string Message { get; set; } = "";

    // Siden brukeren sendes til ved klikk, f. eks. /match/overview
    public string? Link { get; set; }

    public bool IsRead { get; set; }

    // Lagres i UTC, på samme måte som MatchedAt
    public DateTime CreatedAt { get; set; }
}