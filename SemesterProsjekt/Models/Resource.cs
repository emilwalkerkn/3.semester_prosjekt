namespace SemesterProsjekt.Models;

public class Resource
{
    public int ResourceId { get; set; }

    public int UserId { get; set; }

    public int ResourceTypeId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string? GeometryType { get; set; }

    public string? GeometryData { get; set; }

    public string? ContactPoint { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableTo { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = "";

    public string Status { get; set; } = "Available";
}
