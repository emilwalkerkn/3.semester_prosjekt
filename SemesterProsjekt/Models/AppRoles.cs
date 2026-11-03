namespace SemesterProsjekt.Models;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string ResourceOwner = "Ressurseier";
    public const string NeedsActor = "Behovsaktør";

    public static readonly string[] SelfServiceRoles =
    [
        ResourceOwner,
        NeedsActor
    ];
}
