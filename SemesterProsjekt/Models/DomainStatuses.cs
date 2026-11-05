namespace SemesterProsjekt.Models;

/// <summary>
/// Gyldige statusverdier for et behov.
/// </summary>
public static class NeedStatuses
{
    public const string New = "New";
    public const string InProgress = "In progress";
    public const string Resolved = "Resolved";

    // Liste over alle statusene, f.eks. til validering eller nedtrekksmenyer.
    public static readonly string[] All = [New, InProgress, Resolved];
}

/// <summary>
/// Gyldige statusverdier for en match mellom behov og ressurs.
/// Verdiene lagres som tekst i Status-kolonnen i Match-tabellen.
/// </summary>
public static class MatchStatuses
{
    public const string Proposed = "Foreslått";   // Standard for nye matcher
    public const string Approved = "Godkjent";
    public const string Completed = "Fullført";
    public const string Declined = "Avslått";
    public const string Delivered = "Levert";
    public const string Cancelled = "Avbrutt";

    public static readonly string[] All = [Proposed, Approved, Delivered, Completed, Declined, Cancelled];

    public static string DisplayName(string status) => status switch
    {
        Proposed => "Venter på svar", Approved => "Avtalt", _ => status
    };
}
