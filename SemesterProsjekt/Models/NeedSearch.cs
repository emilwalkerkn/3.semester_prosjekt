namespace SemesterProsjekt.Models;

/// Brukes til å søke og filtrere registrerte behov.
/// Filtrerer etter kategori, status, prioritet og søketekst.
public static class NeedSearch
{
    /// Filtrerer listen med behov basert på brukerens søk og valgte filtre.
    public static IEnumerable<Need> Apply(
        IEnumerable<Need> needs,
        string? search,
        int? typeId,
        string? status,
        string? priority)
    {
        // Filtrerer behov etter valgt kategori.
        if (typeId.HasValue)
            needs = needs.Where(n => n.NeedTypeId == typeId);

        // Filtrerer behov etter status.
        if (!string.IsNullOrEmpty(status))
            needs = needs.Where(n => n.Status == status);

        // Filtrerer behov etter prioritet.
        if (!string.IsNullOrEmpty(priority))
            needs = needs.Where(n => n.Priority == priority);

        // Fjerner unødvendige mellomrom fra søketeksten.
        var term = search?.Trim();

        // Søker i tittel, beskrivelse og kategorinavn.
        // Ignorerer forskjellen mellom store og små bokstaver.
        if (!string.IsNullOrEmpty(term))
            needs = needs.Where(n =>
                n.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                n.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (n.NeedTypeName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));

        // Returnerer behovene som passer med søket og filtrene.
        return needs;
    }
}