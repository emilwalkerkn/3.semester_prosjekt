namespace SemesterProsjekt.Models;

// ViewModel som inneholder informasjon om en ressurs
// som registreres gjennom ressursskjemaet.
public class ResourceViewModel
{
    // Type ressurs som tilbys.
    public string Type { get; set; } = "";
    
    // Beskrivelse av ressursen.
    public string Description { get; set; } = "";
   
    // Stedet hvor ressursen befinner seg.
    public string Location { get; set; } = "";
    
    // Latitude valgt fra kartet.
    public string Latitude { get; set; } = "";
    
    // Longitude valgt fra kartet.
    public string Longitude { get; set; } = "";
    
    public string GeometryType { get; set; } = "";
   
    public string GeometryData { get; set; } = "";

}


