using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SemesterProsjekt.Models;

// Informasjon fra ressursskjemaet.
// Brukes både ved oppretting og redigering. Kontrolleren kopierer
// verdiene over til et Resource-objekt før det lagres.
public class ResourceViewModel
{
    // 0 for nye ressurser, ellers ID-en til ressursen som redigeres.
    public int ResourceId { get; set; }

    // Range(1, ...) sikrer at en ressurstype faktisk er valgt.
    [Range(1, int.MaxValue, ErrorMessage = "Velg en ressurstype.")]
    public int ResourceTypeId { get; set; }

    // Valgene i nedtrekksmenyen for ressurstype.
    public IEnumerable<SelectListItem> ResourceTypes { get; set; } = [];

    [Required(ErrorMessage = "Tittel må fylles ut.")]
    [StringLength(160)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Beskrivelse må fylles ut.")]
    public string Description { get; set; } = "";

    // Fylles når brukeren markerer en plassering på kartet i skjemaet.
    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    [StringLength(50)]
    public string GeometryType { get; set; } = "";

    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    public string GeometryData { get; set; } = "";

    // Resten av feltene tilsvarer feltene i Resource.
    [StringLength(255)]
    public string? ContactPoint { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableTo { get; set; }

    public decimal? Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    // Velges i skjemaet: "Available" eller "Unavailable".
    [Required(ErrorMessage = "Status må fylles ut.")]
    [StringLength(50)]
    public string Status { get; set; } = "Available";
}
