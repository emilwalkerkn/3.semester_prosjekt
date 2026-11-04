using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class Match
{
    public int MatchId { get; set; }

    public int NeedId { get; set; }

    public int ResourceId { get; set; }

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

    public DateTime MatchedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? Notes { get; set; }
}
