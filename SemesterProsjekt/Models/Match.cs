using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class Match
{
    public int MatchId { get; set; }

    public int NeedId { get; set; }

    public int ResourceId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "";

    public DateTime MatchedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? Notes { get; set; }
}