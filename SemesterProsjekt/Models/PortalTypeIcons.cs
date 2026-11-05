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
        1 => new("/images/types/food.svg", "Mat og drikke"),
        2 => new("/images/types/transport.svg", "Transport"),
        3 => new("/images/types/medical.svg", "Medisinsk utstyr"),
        4 => new("/images/types/people.svg", "Mannskap"),
        5 => new("/images/types/bed.svg", "Overnatting"),
        _ => new("/images/types/other.svg", "Annet behov")
    };

    // Samme som over, men for ressurstyper (ResourceType-tabellen).
    public static PortalTypeIcon ForResource(int resourceTypeId) => resourceTypeId switch
    {
        1 => new("/images/types/transport.svg", "Kjøretøy"),
        2 => new("/images/types/medical.svg", "Medisinsk utstyr"),
        3 => new("/images/types/food.svg", "Matforsyning"),
        4 => new("/images/types/people.svg", "Mannskap"),
        _ => new("/images/types/other.svg", "Annen ressurs")
    };
}
