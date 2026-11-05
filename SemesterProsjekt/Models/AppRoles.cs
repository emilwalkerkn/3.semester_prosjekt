namespace SemesterProsjekt.Models;

/// <summary>
/// Samler navnene på rollene i portalen på ett sted, slik at vi slipper
/// å skrive rollenavnene som løse tekststrenger rundt i koden.
/// </summary>
public static class AppRoles
{
    // Administrator med full tilgang i portalen.
    public const string Admin = "Admin";

    // Aktør som registrerer ressurser den kan tilby.
    public const string ResourceOwner = "Ressurseier";

    // Aktør som registrerer behov den trenger hjelp til.
    public const string NeedsActor = "Behovsaktør";

    // Eksisterende Identity-rollenavn beholdes så kontoene beholder tilgangen.
    public const string PublicActorDisplayName = "Offentlig aktør";
    public static string DisplayName(string role) => role == NeedsActor ? PublicActorDisplayName : role;

    // Rollene en bruker selv kan velge ved registrering.
    // Admin er bevisst utelatt, så ingen kan gjøre seg selv til administrator.
    public static readonly string[] SelfServiceRoles =
    [
        ResourceOwner,
        NeedsActor
    ];
}
