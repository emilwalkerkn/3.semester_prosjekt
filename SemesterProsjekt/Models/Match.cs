using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

/// <summary>
/// En kobling mellom ett behov og én ressurs. Speiler Match-tabellen i databasen.
/// </summary>
public class Match
{
    // Primærnøkkel, genereres av databasen.
    public int MatchId { get; set; }

    // Fremmednøkler til behovet og ressursen som er koblet sammen.
    public int NeedId { get; set; }

    public int ResourceId { get; set; }

    // Hvor langt matchen har kommet. Se MatchStatuses for gyldige verdier.
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = MatchStatuses.Proposed;

    // Når matchen ble opprettet.
    public DateTime MatchedAt { get; set; }

    // Når matchen ble avsluttet. Null så lenge den fortsatt er aktiv.
    public DateTime? ResolvedAt { get; set; }

    // Valgfri merknad til matchen.
    public string? Notes { get; set; }
}
