namespace SemesterProsjekt.Models;

/// Representerer et ikon og en beskrivelse som brukes i portalen.
public record PortalTypeIcon(string Symbol, string Label);

/// Henter riktige ikoner og navn til forskjellige behov og ressurser.
public static class PortalTypeIcons
{
    // Finner riktig ikon og navn basert på behovets kategori.
    public static PortalTypeIcon ForNeed(int id) =>
        ForCategory(
            CategoryCatalog.Needs.FirstOrDefault(c => c.Id == id),
            "Annet behov");

    // Finner riktig ikon og navn basert på ressursens kategori.
    public static PortalTypeIcon ForResource(int id) =>
        ForCategory(
            CategoryCatalog.Resources.FirstOrDefault(c => c.Id == id),
            "Annen ressurs");

    // Lager en ikonbane og et kategorinavn.
    // Bruker standardikon og standardnavn dersom kategorien ikke finnes.
    private static PortalTypeIcon ForCategory(
        PortalCategory? category,
        string fallback) =>
        new(
            $"/images/types/{category?.Icon ?? "other"}.svg",
            category?.Name ?? fallback);
}