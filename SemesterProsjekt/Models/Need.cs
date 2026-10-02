namespace SemesterProsjekt.Models;

public class Need
{
    public int Id { get; set; }

    public string Type { get; set; } = "";

    public string Description { get; set; } = "";

    public string Location { get; set; } = "";

    public string Latitude { get; set; } = "";

    public string Longitude { get; set; } = "";
    
    public string GeometryType { get; set; } = "";
    
    public string GeometryData { get; set; } = "";
    
    public string Status { get; set; } = "New";
}