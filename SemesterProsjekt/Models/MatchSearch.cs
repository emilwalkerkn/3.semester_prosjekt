using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Models;

public static class MatchSearch
{
    // Brukes etter innsynskontroll og beregning av brukerens mulige handlinger.
    public static IEnumerable<MatchListItemViewModel> Apply(IEnumerable<MatchListItemViewModel> matches, string? search, string? status, bool requiresAction)
    {
        if (!string.IsNullOrWhiteSpace(status)) matches = matches.Where(m => m.Status == status);
        if (requiresAction) matches = matches.Where(m => m.CanRespond || m.CanDeliver || m.CanComplete);
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term))
            matches = matches.Where(m => new[] { m.NeedTitle, m.ResourceTitle, m.RequesterName, m.SupplierName, m.DeliveryLocation, m.MatchId.ToString() }
                .Any(value => value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true));
        return matches;
    }
}
