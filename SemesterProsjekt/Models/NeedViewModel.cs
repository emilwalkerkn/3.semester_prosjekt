namespace SemesterProsjekt.Models;


// ViewModel som inneholder informasjonen som sendes
// mellom behovsskjemaet, controlleren og resultatsiden.
public class NeedViewModel
{
    
    // Type behov som registreres.
    public string Type { get; set; } = "";
    
    // Beskrivelse av behovet.
    public string Description { get; set; } = "";
    
    // Stedet hvor behovet gjelder.
    public string Location { get; set; } = "";
    
    // Latitude valgt fra Leaflet-kartet.
    public string Latitude { get; set; } = "";
    
    // Longitude valgt fra Leaflet-kartet.
    public string Longitude { get; set; } = "";
    
    public string GeometryType { get; set; } = "";
    
    public string GeometryData { get; set; } = "";
  
    public string Status { get; set; } = "New";
}