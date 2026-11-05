namespace SemesterProsjekt.Models;

public record PortalCategory(int Id, string Name, string Icon);

// Faste ID-er bevarer eksisterende registreringer. Nye kategorier må finnes
// både i databasen, ikonoppslaget og koblingen mellom behov og ressurs.
public static class CategoryCatalog
{
    public const int OtherId = 99;
    public static readonly IReadOnlyList<PortalCategory> Needs = new PortalCategory[]
    {
        new(1, "Mat og drikke", "food"), new(2, "Kjøretøy", "transport"),
        new(3, "Medisinsk utstyr", "medical"), new(4, "Mannskap", "people"),
        new(5, "Overnatting", "bed"),
        new(6, "Strøm og belysning", "power"), new(7, "Varme og bekledning", "warmth"),
        new(8, "Kommunikasjon", "communication"), new(9, "Hygiene og sanitær", "hygiene"),
        new(10, "Verktøy og maskiner", "tools"), new(OtherId, "Annet", "other")
    };
    // Felles navn og rekkefølge; bare gamle database-ID-er er forskjellige.
    public static readonly IReadOnlyList<PortalCategory> Resources = Needs
        .Select(category => new PortalCategory(ResourceTypeFor(category.Id)!.Value, category.Name, category.Icon))
        .ToArray();

    public static int? ResourceTypeFor(int needType) => needType switch
    {
        1 => 3, 2 => 1, 3 => 2, 4 => 4, 5 => 5,
        6 => 6, 7 => 7, 8 => 8, 9 => 9, 10 => 10, OtherId => OtherId, _ => null
    };
}
