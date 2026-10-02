namespace SemesterProsjekt.Models;

public class Resource
{
    public int Id { get; set; }

    public string Type { get; set; } = "";

    public string Description { get; set; } = "";

    public string Location { get; set; } = "";

    public string Latitude { get; set; } = "";

    public string Longitude { get; set; } = "";
}