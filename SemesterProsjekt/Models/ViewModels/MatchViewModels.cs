using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SemesterProsjekt.Models.ViewModels;

public class CreateMatchViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Velg et behov.")]
    public int NeedId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Velg en ressurs.")]
    public int ResourceId { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Mengden må være større enn null.")]
    public decimal? AssignedQuantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> Needs { get; set; } = [];
    public IEnumerable<SelectListItem> Resources { get; set; } = [];
}

public class MatchListItemViewModel
{
    public int MatchId { get; set; }
    public int NeedId { get; set; }
    public string NeedTitle { get; set; } = string.Empty;
    public int ResourceId { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;
    public decimal? AssignedQuantity { get; set; }
    public string? AssignedUnit { get; set; }
    public string NeedUserId { get; set; } = string.Empty;
    public string ResourceUserId { get; set; } = string.Empty;
    public string? ProposedByUserId { get; set; }

    // Beregnes i controlleren fra innlogging og eierskap, ikke fra skjemafelter.
    public bool CanRespond { get; set; }
    public bool CanComplete { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime MatchedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Notes { get; set; }
}
