using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SemesterProsjekt.Models.ViewModels;

/// <summary>
/// Skjemaet for å opprette en ny match mellom et behov og en ressurs.
/// </summary>
public class CreateMatchViewModel
{
    // Range(1, ...) sikrer at brukeren faktisk har valgt noe (0 = ingenting valgt).
    [Range(1, int.MaxValue, ErrorMessage = "Velg et behov.")]
    public int NeedId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Velg en ressurs.")]
    public int ResourceId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Valgene i nedtrekksmenyene for behov og ressurser.
    public IEnumerable<SelectListItem> Needs { get; set; } = [];
    public IEnumerable<SelectListItem> Resources { get; set; } = [];
}

/// <summary>
/// Én rad i oversikten over matcher. Inneholder titlene på behovet
/// og ressursen, så visningen slipper å slå dem opp selv.
/// </summary>
public class MatchListItemViewModel
{
    public int MatchId { get; set; }
    public int NeedId { get; set; }
    public string NeedTitle { get; set; } = string.Empty;
    public int ResourceId { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime MatchedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Notes { get; set; }
}
