using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

// Informasjon fra behovsskjemaet.
public class NeedViewModel
{
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

    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    [StringLength(50)]
    public string? GeometryType { get; set; }

    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    public string? GeometryData { get; set; }

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

    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = "Normal";
}