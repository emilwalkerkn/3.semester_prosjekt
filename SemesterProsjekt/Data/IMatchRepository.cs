using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Data;


/// Definerer operasjonene for å hente, opprette og oppdatere matcher.
/// Tilbyr både samlet oversikt for admin og filtrert oversikt for brukere.

public interface IMatchRepository
{

    /// Henter aktive forespørsler, avtalte leveranser og leveranser som venter på mottak.

    Task<IEnumerable<MatchListItemViewModel>> GetActiveAsync();


    /// Henter fullførte, avslåtte og avbrutte avtaler til administratorens historikk.

    Task<IEnumerable<MatchListItemViewModel>> GetHistoryAsync();


    /// Henter åpne matchforslag der brukeren eier behovet eller ressursen.

    Task<IEnumerable<MatchListItemViewModel>> GetActiveForUserAsync(string userId);


    /// Henter behandlede matcher der brukeren eier behovet eller ressursen.

    Task<IEnumerable<MatchListItemViewModel>> GetHistoryForUserAsync(string userId);


    /// Henter én match, eller null dersom den ikke finnes.

    Task<Match?> GetByIdAsync(int id);

    /// Henter reserverte mengder slik at opptatte registreringer ikke tilbys på nytt.
    Task<IEnumerable<MatchReservation>> GetReservationsAsync();


    /// Undersøker om kombinasjonen allerede har en aktiv forespørsel eller avtale.

    Task<bool> ExistsAsync(int needId, int resourceId);


    /// Lagrer en ny match og returnerer den genererte ID-en.

    Task<int> CreateAsync(Match match);

    /// Oppdaterer bare dersom statusen fortsatt er den controlleren kontrollerte.
    /// Hindrer at samtidige svar overskriver hverandre.
   
    Task<bool> TryUpdateStatusAsync(
        int id, string expectedStatus, string status, DateTime? resolvedAt);
}
