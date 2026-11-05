namespace SemesterProsjekt.Models;

public record PortalTypeIcon(string Symbol, string Label);

public static class PortalTypeIcons
{
    public static PortalTypeIcon ForNeed(int id) => ForCategory(CategoryCatalog.Needs.FirstOrDefault(c => c.Id == id), "Annet behov");
    public static PortalTypeIcon ForResource(int id) => ForCategory(CategoryCatalog.Resources.FirstOrDefault(c => c.Id == id), "Annen ressurs");
    private static PortalTypeIcon ForCategory(PortalCategory? category, string fallback) =>
        new($"/images/types/{category?.Icon ?? "other"}.svg", category?.Name ?? fallback);
}
