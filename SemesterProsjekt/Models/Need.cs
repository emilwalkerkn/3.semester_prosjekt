using System.ComponentModel.DataAnnotations;
namespace SemesterProsjekt.Models;

/// <summary>
/// Et behov registrert av en behovsaktør. Speiler Needs-tabellen i databasen.
/// Attributtene ([Required], [StringLength]) validerer data og matcher kolonnegrensene.
/// </summary>
public class Need
{
    // Primærnøkkel, genereres av databasen.
    public int NeedId { get; set; }

    // Identity-ID-en til brukeren som registrerte behovet.
    [Required]
    [StringLength(255)]
    public string UserId { get; set; } = string.Empty;

    // Fremmednøkkel til NeedType (kategori, f.eks. "Transport").
    public int NeedTypeId { get; set; }

    // Fylles av repositoryet når behovet hentes sammen med sin type.
    public string? NeedTypeName { get; set; }

    [Required(ErrorMessage = "Tittel må fylles ut.")]
    [StringLength(160)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Beskrivelse må fylles ut.")]
    public string Description { get; set; } = "";

    // Plassering på kartet: typen (punkt eller linje) og selve geometrien som GeoJSON.
    [Required]
    [StringLength(50)]
    public string GeometryType { get; set; } = "";

    [Required]
    public string GeometryData { get; set; } = "";

    // Valgfri kontaktinformasjon for behovet.
    [StringLength(255)]
    public string? ContactPoint { get; set; }

    // Valgfritt tidsrom: når behovet oppstår, og når det må være dekket.
    public DateTime? NeededFrom { get; set; }

    public DateTime? Deadline { get; set; }

    // Valgfri mengde og enhet, f.eks. 20 + "liter".
    public decimal? Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    // Oppfølgingsstatus. Se NeedStatuses for gyldige verdier.
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "New";

    // Hvor viktig behovet er: Low, Normal, High eller Critical.
    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = "Normal";
}
