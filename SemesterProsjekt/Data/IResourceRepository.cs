using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Definerer operasjonene for å hente og behandle ressurser og ressurstyper.
/// Gjør at controllerne kan bruke databaselaget uten å kjenne SQL-implementasjonen.

public interface IResourceRepository
{
   
    /// Henter alle ressurser med tilhørende ressurstypenavn.
  
    Task<IEnumerable<Resource>> GetAllAsync();

   
    /// Henter ressursen med angitt ID.
    /// Returnerer null dersom ressursen ikke finnes.
  
    Task<Resource?> GetByIdAsync(int id);

   
    /// Lagrer en ny ressurs og returnerer den genererte ID-en.
  
    Task<int> CreateAsync(Resource resource);

   
    /// Oppdaterer ressursen med angitt ResourceId uten å endre eieren.
  
    Task UpdateAsync(Resource resource);

   
    /// Sletter ressursen med angitt ID.
  
    Task DeleteAsync(int id);

   
    /// Henter tilgjengelige ressurstyper alfabetisk etter navn.
  
    Task<IEnumerable<ResourceType>> GetResourceTypesAsync();
}