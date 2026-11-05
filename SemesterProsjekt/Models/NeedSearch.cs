namespace SemesterProsjekt.Models;

public static class NeedSearch
{
    public static IEnumerable<Need> Apply(IEnumerable<Need> needs, string? search, int? typeId, string? status, string? priority)
    {
        if (typeId.HasValue) needs = needs.Where(n => n.NeedTypeId == typeId);
        if (!string.IsNullOrEmpty(status)) needs = needs.Where(n => n.Status == status);
        if (!string.IsNullOrEmpty(priority)) needs = needs.Where(n => n.Priority == priority);
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term))
            needs = needs.Where(n =>
                n.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                n.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (n.NeedTypeName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        return needs;
    }
}
