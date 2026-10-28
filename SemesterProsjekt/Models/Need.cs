using System.ComponentModel.DataAnnotations;
namespace SemesterProsjekt.Models;

public class Need
{
    public int NeedId { get; set; }

    public int UserId { get; set; }

    public int NeedTypeId { get; set; }

    [Required(ErrorMessage = "Tittel må fylles ut.")]
    [StringLength(160)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Beskrivelse må fylles ut.")]
    public string Description { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string GeometryType { get; set; } = "";

    [Required]
    public string GeometryData { get; set; } = "";

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