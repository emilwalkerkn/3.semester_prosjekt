namespace SemesterProsjekt.Models;

// Kobler et registrert behov til en ressurs som kan dekke hele eller deler av behovet.
public class ResourceMatch
{
    public int MatchId { get; set; }
    public int NeedId { get; set; }
    public int ResourceId { get; set; }
    public int NeedTypeId { get; set; }
    public int ResourceTypeId { get; set; }
    public decimal AssignedQuantity { get; set; }
    public string Status { get; set; } = "Proposed";
    public DateTime CreatedAt { get; set; }

    // Hentes fra de tilknyttede tabellene og brukes i oversikten.
    public string NeedTitle { get; set; } = "";
    public string ResourceTitle { get; set; } = "";
    public string Unit { get; set; } = "";
    public int NeedOwnerId { get; set; }
    public int ResourceOwnerId { get; set; }
}
