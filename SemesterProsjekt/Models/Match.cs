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
    // Foreslått mengde låses ved godkjenning og trekkes fra ved fullføring.
    public decimal? AssignedQuantity { get; set; }
    public string? AssignedUnit { get; set; }

 
    /// Brukeren som opprettet forslaget. Settes fra innloggingen.
    /// Eldre matcher kan mangle denne opplysningen og behandles da av admin.
   
    [StringLength(255)]
    public string? ProposedByUserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = MatchStatuses.Proposed;

    // Når matchen ble opprettet.
    public DateTime MatchedAt { get; set; }

    // Når matchen ble avsluttet. Null så lenge den fortsatt er aktiv.
    public DateTime? ResolvedAt { get; set; }

    // Valgfri merknad til matchen.
    public string? Notes { get; set; }
    public DateTime? DeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryLocation { get; set; }
    public string? DeliveryContact { get; set; }
    public string? RequestNeedTitle { get; set; }
    public string? RequestResourceTitle { get; set; }
}
