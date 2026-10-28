using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

// Informasjon fra ressursskjemaet.
public class ResourceViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Velg en ressurstype.")]
    public int ResourceTypeId { get; set; }

    [Required(ErrorMessage = "Tittel må fylles ut.")]
    [StringLength(160)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Beskrivelse må fylles ut.")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    [StringLength(50)]
    public string GeometryType { get; set; } = "";

    [Required(ErrorMessage = "Velg en plassering på kartet.")]
    public string GeometryData { get; set; } = "";

    [StringLength(255)]
    public string? ContactPoint { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableTo { get; set; }

    public decimal? Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [Required(ErrorMessage = "Status må fylles ut.")]
    [StringLength(50)]
    public string Status { get; set; } = "Available";
}