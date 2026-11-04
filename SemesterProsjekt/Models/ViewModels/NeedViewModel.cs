using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

// Informasjon fra behovsskjemaet.
// Brukes både ved oppretting og redigering. Kontrolleren kopierer
// verdiene over til et Need-objekt før det lagres.
public class NeedViewModel
{
    // 0 for nye behov, ellers ID-en til behovet som redigeres.
    public int NeedId { get; set; }

    // Nullable slik at [Required] slår inn når ingen type er valgt.
    [Required(ErrorMessage = "Velg type behov")]
    [Display(Name = "Type behov")]
    public int? NeedTypeId { get; set; }

    // Samling av dropdown-valg
    public IEnumerable<SelectListItem> NeedTypes { get; set; }
        = Enumerable.Empty<SelectListItem>();

    [Required(ErrorMessage = "Tittel må fylles ut.")]
    [StringLength(160)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Beskrivelse må fylles ut.")]
    public string Description { get; set; } = "";

    // Fylles når brukeren markerer en plassering på kartet i skjemaet.
    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    [StringLength(50)]
    public string? GeometryType { get; set; }

    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    public string? GeometryData { get; set; }

    // Resten av feltene tilsvarer feltene i Need.
    [StringLength(255)]
    public string? ContactPoint { get; set; }

    public DateTime? NeededFrom { get; set; }

    public DateTime? Deadline { get; set; }

    public decimal? Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "New";

    // Velges i skjemaet: Low, Normal, High eller Critical.
    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = "Normal";
}
