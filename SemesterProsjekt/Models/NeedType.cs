using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class NeedType
{
    public int NeedTypeId { get; set; }

    [Required(ErrorMessage = "Navn på behovstype må fylles ut.")]
    [StringLength(80)]
    public string Name { get; set; } = "";
}