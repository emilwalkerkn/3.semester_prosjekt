using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Definerer operasjonene for å hente og behandle behov og behovstyper.
/// Gjør at controllerne kan bruke databaselaget uten å kjenne SQL-implementasjonen.

public interface INeedRepository
{
  
    /// Henter alle behov med tilhørende behovstypenavn.
    /// Støtter sortering etter frist, prioritet eller status.
    /// Standardvalget viser behovene med høyest ID først.
   
    Task<IEnumerable<Need>> GetAllAsync(string sortBy = "newest");

  
    /// Henter behovet med angitt ID.
    /// Returnerer null dersom behovet ikke finnes.
   
    Task<Need?> GetByIdAsync(int id);

  
    /// Lagrer et nytt behov og returnerer den genererte ID-en.
   
    Task<int> CreateAsync(Need need);

  
    /// Oppdaterer behovet med angitt NeedId uten å endre eieren.
   
    Task UpdateAsync(Need need);

  
    /// Sletter behovet med angitt ID.
   
    Task DeleteAsync(int id);

  
    /// Oppdaterer bare statusen til behovet med angitt ID.
    /// Tillatt status og brukerens tilgang må kontrolleres før metoden kalles.
   
    Task UpdateStatusAsync(int id, string status);

  
    /// Henter tilgjengelige behovstyper alfabetisk etter navn.
   
    Task<IEnumerable<NeedType>> GetNeedTypesAsync();
}