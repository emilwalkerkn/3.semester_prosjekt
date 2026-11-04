using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Data;


/// Definerer operasjonene for å hente, opprette og oppdatere matcher.
/// Gjør at controlleren kan bruke databaselaget uten å kjenne SQL-implementasjonen.

public interface IMatchRepository
{
   
    /// Henter matcher som verken er fullført eller avslått.
    /// Resultatet inkluderer behovs- og ressurstitler til oversiktsvisningen.
  
    Task<IEnumerable<MatchListItemViewModel>> GetActiveAsync();

   
    /// Henter fullførte og avslåtte matcher med behovs- og ressurstitler
    /// til historikkvisningen.
  
    Task<IEnumerable<MatchListItemViewModel>> GetHistoryAsync();

   
    /// Henter matchen med angitt ID.
    /// Returnerer null dersom matchen ikke finnes.
  
    Task<Match?> GetByIdAsync(int id);

   
    /// Undersøker om kombinasjonen av behov og ressurs allerede finnes,
    /// uavhengig av matchens status.
  
    Task<bool> ExistsAsync(int needId, int resourceId);

   
    /// Lagrer en ny match og returnerer den genererte ID-en.
  
    Task<int> CreateAsync(Match match);

   
    /// Oppdaterer status og avslutningstidspunkt for matchen med angitt ID.
    /// Avslutningstidspunktet kan være null for en aktiv match.
    /// Tilgang og tillatt statusovergang må kontrolleres før metoden kalles.
  
    Task UpdateStatusAsync(int id, string status, DateTime? resolvedAt);
}