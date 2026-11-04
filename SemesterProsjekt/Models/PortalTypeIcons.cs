namespace SemesterProsjekt.Models;

// Et ikon (emoji) og en tekst som beskriver en kategori.
// "record" gir en enkel, uforanderlig dataklasse på én linje.
public record PortalTypeIcon(string Symbol, string Label);

// Ett felles oppslag gjør at samme kategori får samme ikon overalt i portalen.
public static class PortalTypeIcons
{
    // Finner ikon for en behovstype ut fra ID-en.
    // ID-ene samsvarer med radene som legges inn i NeedType-tabellen i init.sql.
    // "_" er standardtilfellet for ukjente ID-er.
    public static PortalTypeIcon ForNeed(int needTypeId) => needTypeId switch
    {
        1 => new("🍽️", "Mat og drikke"),
        2 => new("🚐", "Transport"),
        3 => new("🩺", "Medisinsk utstyr"),
        4 => new("👥", "Mannskap"),
        5 => new("🛏️", "Overnatting"),
        _ => new("📌", "Annet behov")
    };

    // Samme som over, men for ressurstyper (ResourceType-tabellen).
    public static PortalTypeIcon ForResource(int resourceTypeId) => resourceTypeId switch
    {
        1 => new("🚐", "Kjøretøy"),
        2 => new("🩺", "Medisinsk utstyr"),
        3 => new("🍽️", "Matforsyning"),
        4 => new("👥", "Mannskap"),
        _ => new("📦", "Annen ressurs")
    };
}
