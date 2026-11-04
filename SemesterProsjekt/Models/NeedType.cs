using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

/// <summary>
/// En kategori av behov (f.eks. "Mat og drikke"). Speiler NeedType-tabellen
/// og brukes som valg i nedtrekksmenyen når et behov registreres.
/// </summary>
public class NeedType
{
    // Primærnøkkel, genereres av databasen.
    public int NeedTypeId { get; set; }

    // Navnet på kategorien. Må være unikt i databasen.
    [Required(ErrorMessage = "Navn på behovstype må fylles ut.")]
    [StringLength(80)]
    public string Name { get; set; } = "";
}
