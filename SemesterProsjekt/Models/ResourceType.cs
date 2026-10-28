using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class ResourceType
{
    public int ResourceTypeId { get; set; }

    [Required(ErrorMessage = "Navn på ressurstyype må fylles ut.")]
    [StringLength(80)]
    public string Name { get; set; } = "";
    
}