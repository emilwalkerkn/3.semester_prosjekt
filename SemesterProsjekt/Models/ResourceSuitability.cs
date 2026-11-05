namespace SemesterProsjekt.Models;

public static class ResourceSuitability
{
    // Type-ID-ene har forskjellig betydning i behovs- og ressurstabellen.
    public static int? ResourceTypeFor(int needType) => needType switch { 1 => 3, 2 => 1, 3 => 2, 4 => 4, _ => null };
    public static bool IsSuitable(Need need, Resource resource, IEnumerable<MatchReservation> reservations)
    {
        if (ResourceTypeFor(need.NeedTypeId) is not { } type || resource.ResourceTypeId != type) return false;
        var time = need.NeededFrom > DateTime.Now ? need.NeededFrom : DateTime.Now;
        if ((resource.AvailableFrom.HasValue && resource.AvailableFrom > time) ||
            (resource.AvailableTo.HasValue && resource.AvailableTo < time)) return false;
        try { MatchAvailability.Allocate(need, resource, reservations, null); return true; }
        catch (MatchAvailabilityException) { return false; }
    }
}
