namespace SemesterProsjekt.Models;

public record PortalTypeIcon(string Symbol, string Label);

// Ett felles oppslag gjør at samme kategori får samme ikon overalt i portalen.
public static class PortalTypeIcons
{
    public static PortalTypeIcon ForNeed(int needTypeId) => needTypeId switch
    {
        1 => new("🍽️", "Mat og drikke"),
        2 => new("🚐", "Transport"),
        3 => new("🩺", "Medisinsk utstyr"),
        4 => new("👥", "Mannskap"),
        5 => new("🛏️", "Overnatting"),
        _ => new("📌", "Annet behov")
    };

    public static PortalTypeIcon ForResource(int resourceTypeId) => resourceTypeId switch
    {
        1 => new("🚐", "Kjøretøy"),
        2 => new("🩺", "Medisinsk utstyr"),
        3 => new("🍽️", "Matforsyning"),
        4 => new("👥", "Mannskap"),
        _ => new("📦", "Annen ressurs")
    };
}
