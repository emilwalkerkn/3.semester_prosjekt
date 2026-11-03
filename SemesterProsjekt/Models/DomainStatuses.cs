namespace SemesterProsjekt.Models;

public static class NeedStatuses
{
    public const string New = "New";
    public const string InProgress = "In progress";
    public const string Resolved = "Resolved";

    public static readonly string[] All = [New, InProgress, Resolved];
}

public static class MatchStatuses
{
    public const string Proposed = "Foreslått";
    public const string Approved = "Godkjent";
    public const string Completed = "Fullført";

    public static readonly string[] All = [Proposed, Approved, Completed];
}
