using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SemesterProsjekt.Models.ViewModels;

/// Brukes til å registrere en ny kobling mellom et behov og en ressurs.
/// Inneholder informasjonen som brukeren fyller ut i skjemaet.
public class CreateMatchViewModel
{
    // Sikrer at brukeren velger et gyldig behov og en gyldig ressurs.
    // Verdien 0 betyr at ingenting er valgt.
    [Range(1, int.MaxValue, ErrorMessage = "Velg et behov.")]
    public int NeedId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Velg en ressurs.")]
    public int ResourceId { get; set; }

    // Kontrollerer at mengden er positiv og innenfor tillatt område.
    [Range(typeof(decimal), "0.01", "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Mengden må være større enn null.")]
    public decimal? AssignedQuantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Inneholder valgene som vises i nedtrekksmenyene.
    public IEnumerable<SelectListItem> Needs { get; set; } = [];
    public IEnumerable<SelectListItem> Resources { get; set; } = [];
}

/// Representerer én match i oversikten over behov og ressurser.
/// Inneholder informasjon om status, levering og hvilke handlinger brukeren kan utføre.
public class MatchListItemViewModel
{
    // Identifiserer matchen og behovet og ressursen som er koblet sammen.
    public int MatchId { get; set; }
    public int NeedId { get; set; }
    public string NeedTitle { get; set; } = string.Empty;
    public int ResourceId { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;

    // Mengde og enhet som er knyttet til matchen.
    public decimal? AssignedQuantity { get; set; }
    public string? AssignedUnit { get; set; }

    // Identifiserer brukerne som eier behovet og ressursen.
    public string NeedUserId { get; set; } = string.Empty;
    public string ResourceUserId { get; set; } = string.Empty;
    public string? ProposedByUserId { get; set; }

    // Controlleren bestemmer hvilke handlinger brukeren har tilgang til.
    // Verdiene beregnes ut fra innlogging og eierskap.
    public bool CanRespond { get; set; }
    public bool CanComplete { get; set; }
    public bool CanDeliver { get; set; }
    public bool CanCancel { get; set; }

    // Informasjon om planlagt og gjennomført levering.
    public DateTime? DeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryLocation { get; set; }
    public string? DeliveryContact { get; set; }

    // Navnene på partene som er involvert i matchen.
    public string? RequesterName { get; set; }
    public string? SupplierName { get; set; }

    // Viser status og hva som er neste steg i prosessen.
    public string NextStep { get; set; } = "";
    public string Status { get; set; } = string.Empty;
    public DateTime MatchedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Notes { get; set; }
}

/// Brukes når brukeren skal finne tilgjengelige ressurser til et behov.
/// Inneholder behovet og en liste over aktuelle ressurser.
public class FindResourcesViewModel
{
    public Need Need { get; set; } = new();
    public List<ResourceCandidate> Candidates { get; set; } = [];
}

/// Representerer en aktuell ressurs og hvor mye som er tilgjengelig.
public record ResourceCandidate(
    Resource Resource,
    decimal? AvailableQuantity);

/// Brukes i skjemaet for å avtale levering av en ressurs.
/// Inneholder mengde, tidspunkt, leveringssted og kontaktinformasjon.
public class DeliveryRequestViewModel
{
    // Identifiserer behovet og ressursen som leveringen gjelder.
    [Range(1, int.MaxValue)]
    public int NeedId { get; set; }

    [Range(1, int.MaxValue)]
    public int ResourceId { get; set; }

    // Kontrollerer at mengden er positiv og har maksimalt to desimaler.
    [Range(typeof(decimal), "0.01", "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Velg en positiv mengde med høyst to desimaler.")]
    public decimal? AssignedQuantity { get; set; }

    // Påkrevd informasjon for å kunne avtale levering.
    [Required(ErrorMessage = "Velg tidspunkt for levering.")]
    public DateTime? DeliveryAt { get; set; }

    [Required(ErrorMessage = "Oppgi leveringssted.")]
    [StringLength(255)]
    public string DeliveryLocation { get; set; } = "";

    [Required(ErrorMessage = "Oppgi kontaktinformasjon for leveringen.")]
    [StringLength(255)]
    public string DeliveryContact { get; set; } = "";

    [StringLength(500)]
    public string? Notes { get; set; }

    // Informasjon som vises i skjemaet, men som ikke skal valideres som brukerinput.
    [ValidateNever]
    public string NeedTitle { get; set; } = "";

    [ValidateNever]
    public string ResourceTitle { get; set; } = "";

    [ValidateNever]
    public string? Unit { get; set; }

    [ValidateNever]
    public string? SupplierContact { get; set; }

    [ValidateNever]
    public decimal? AvailableQuantity { get; set; }
}