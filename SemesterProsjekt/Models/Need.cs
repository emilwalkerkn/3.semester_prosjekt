namespace SemesterProsjekt.Models;

public class Need
{
    public int NeedId { get; set; }
    
    public int UserId { get; set; }
    public int NeedTypeId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string? GeometryType { get; set; }

    public string? GeometryData { get; set; }

    public string? ContactPoint { get; set; }

    public DateTime? NeededFrom { get; set; }

    public DateTime? Deadline { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = "";

    public string Status { get; set; } = "New";

    public string Priority { get; set; } = "Normal";
}