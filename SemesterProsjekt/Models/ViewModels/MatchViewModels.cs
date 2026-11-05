using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Mengden må være større enn null.")]
    public decimal? AssignedQuantity { get; set; }

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
    public decimal? AssignedQuantity { get; set; }
    public string? AssignedUnit { get; set; }
    public string NeedUserId { get; set; } = string.Empty;
    public string ResourceUserId { get; set; } = string.Empty;
    public string? ProposedByUserId { get; set; }

    // Beregnes i controlleren fra innlogging og eierskap, ikke fra skjemafelter.
    public bool CanRespond { get; set; }
    public bool CanComplete { get; set; }
    public bool CanDeliver { get; set; }
    public bool CanCancel { get; set; }
    public DateTime? DeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryLocation { get; set; }
    public string? DeliveryContact { get; set; }
    public string? RequesterName { get; set; }
    public string? SupplierName { get; set; }
    public string NextStep { get; set; } = "";
    public string Status { get; set; } = string.Empty;
    public DateTime MatchedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Notes { get; set; }
}

public class FindResourcesViewModel
{
    public Need Need { get; set; } = new();
    public List<ResourceCandidate> Candidates { get; set; } = [];
}

public record ResourceCandidate(Resource Resource, decimal? AvailableQuantity);

public class DeliveryRequestViewModel
{
    [Range(1, int.MaxValue)] public int NeedId { get; set; }
    [Range(1, int.MaxValue)] public int ResourceId { get; set; }
    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Velg en positiv mengde med høyst to desimaler.")]
    public decimal? AssignedQuantity { get; set; }
    [Required(ErrorMessage = "Velg tidspunkt for levering.")]
    public DateTime? DeliveryAt { get; set; }
    [Required(ErrorMessage = "Oppgi leveringssted.")]
    [StringLength(255)] public string DeliveryLocation { get; set; } = "";
    [Required(ErrorMessage = "Oppgi kontaktinformasjon for leveringen.")]
    [StringLength(255)] public string DeliveryContact { get; set; } = "";
    [StringLength(500)] public string? Notes { get; set; }
    [ValidateNever] public string NeedTitle { get; set; } = "";
    [ValidateNever] public string ResourceTitle { get; set; } = "";
    [ValidateNever] public string? Unit { get; set; }
    [ValidateNever] public string? SupplierContact { get; set; }
    [ValidateNever] public decimal? AvailableQuantity { get; set; }
}
