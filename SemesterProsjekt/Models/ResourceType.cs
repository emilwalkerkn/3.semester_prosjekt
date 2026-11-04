using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

/// <summary>
/// En kategori av ressurser (f.eks. "Kjøretøy"). Speiler ResourceType-tabellen
/// og brukes som valg i nedtrekksmenyen når en ressurs registreres.
/// </summary>
public class ResourceType
{
    // Primærnøkkel, genereres av databasen.
    public int ResourceTypeId { get; set; }

    // Navnet på kategorien. Må være unikt i databasen.
    [Required(ErrorMessage = "Navn på ressurstyype må fylles ut.")]
    [StringLength(80)]
    public string Name { get; set; } = "";

}
