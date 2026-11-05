namespace SemesterProsjekt.Models;

public static class ResourceSearch
{
    // Tilgangsfiltreringen kjøres før søk, så søk kan aldri utvide innsynet.
    public static IEnumerable<Resource> Apply(IEnumerable<Resource> resources, string? search, int? typeId, string? status)
    {
        if (typeId.HasValue) resources = resources.Where(r => r.ResourceTypeId == typeId);
        if (!string.IsNullOrEmpty(status)) resources = resources.Where(r => r.Status == status);
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term))
            resources = resources.Where(r =>
                r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.ResourceTypeName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        return resources;
    }
}
