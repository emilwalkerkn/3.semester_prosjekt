using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace SemesterProsjekt.Models;

// Informasjon fra behovsskjemaet.
public class NeedViewModel
{
    [Required(ErrorMessage = "Velg type behov")]
    [Display(Name= "Type behov")]
    public int? NeedTypeId { get; set; }

    // samling av dropdown valg
    public IEnumerable<SelectListItem> NeedTypes { get; set; }
        = Enumerable.Empty<SelectListItem>();
    
    public int UserId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string? GeometryType { get; set; }

    public string? GeometryData { get; set; }

    public string? ContactPoint { get; set; }

    public DateTime? NeededFrom { get; set; }

    public DateTime? Deadline { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = "";

    public string Status { get; set; } = "New";

    public string Priority { get; set; } = "Normal";
}
