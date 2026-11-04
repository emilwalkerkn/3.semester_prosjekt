using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

/// <summary>
/// En ressurs registrert av en ressurseier. Speiler Resources-tabellen i databasen.
/// Har mye av den samme strukturen som Need.
/// </summary>
public class Resource
{
    // Primærnøkkel, genereres av databasen.
    public int ResourceId { get; set; }


    // Identity-ID-en til ressurseieren.
    [Required]
    [StringLength(255)]
    public string UserId { get; set; } = string.Empty;

    // Fremmednøkkel til ResourceType (kategori, f.eks. "Kjøretøy").
    public int ResourceTypeId { get; set; }

    // Fylles av repositoryet når ressursen hentes sammen med sin type.
    public string? ResourceTypeName { get; set; }

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

    // Valgfri kontaktinformasjon for ressursen.
    [StringLength(255)]
    public string? ContactPoint { get; set; }

    // Valgfritt tidsrom ressursen er tilgjengelig i.
    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableTo { get; set; }

    // Valgfri mengde og enhet, f.eks. 3 + "kjøretøy".
    public decimal? Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    // Om ressursen kan brukes: "Available" eller "Unavailable".
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Available";
}
