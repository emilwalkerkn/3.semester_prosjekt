using Dapper;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Data;


/// Håndterer databasetilgang for matcher mellom behov og ressurser med Dapper.
/// Tilgangskontroll og kontroll av tillatte statusoverganger skjer i controlleren.

public class MatchRepository : IMatchRepository
{
    private readonly IDbConnectionFactory _connectionFactory;


    /// Mottar tilkoblingsfabrikken gjennom dependency injection.
    /// Fabrikken oppretter databasetilkoblingene som brukes av metodene.
 
    public MatchRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }


    /// Henter matcher som verken er fullført eller avslått.
    /// Disse vises i oversikten over aktive matcher.
 
    public Task<IEnumerable<MatchListItemViewModel>> GetActiveAsync() =>
        GetByHistoryAsync(isClosed: false);


    /// Henter fullførte og avslåtte matcher til historikkvisningen.
 
    public Task<IEnumerable<MatchListItemViewModel>> GetHistoryAsync() =>
        GetByHistoryAsync(isClosed: true);


    /// Felles spørring for aktive og avsluttede matcher.
    /// Henter også titlene på behovet og ressursen til visningsmodellen.
    /// Resultatet sorteres med nyeste registreringstidspunkt først.
 
    private async Task<IEnumerable<MatchListItemViewModel>> GetByHistoryAsync(
        bool isClosed)
    {
        // using sørger for at tilkoblingen frigjøres etter bruk.
        using var connection = _connectionFactory.CreateConnection();

        // INNER JOIN henter matcher som har både et tilhørende behov og en ressurs.
        // Statusen avgjør om matchen tilhører aktive matcher eller historikken.
        const string sql = """
            SELECT m.MatchId, m.NeedId, n.Title AS NeedTitle,
                   m.ResourceId, r.Title AS ResourceTitle, m.Status,
                   m.MatchedAt, m.ResolvedAt, m.Notes
            FROM `Match` m
            INNER JOIN Needs n ON n.NeedId = m.NeedId
            INNER JOIN Resources r ON r.ResourceId = m.ResourceId
            WHERE (@IsClosed = 1 AND m.Status IN (@CompletedStatus, @DeclinedStatus))
               OR (@IsClosed = 0 AND m.Status NOT IN (@CompletedStatus, @DeclinedStatus))
            ORDER BY m.MatchedAt DESC;
            """;

        // Verdiene sendes som parametere og behandles som data, ikke SQL-kode.
        // Statusverdiene hentes fra de samme konstantene som controlleren bruker.
        return await connection.QueryAsync<MatchListItemViewModel>(sql, new
        {
            IsClosed = isClosed,
            CompletedStatus = MatchStatuses.Completed,
            DeclinedStatus = MatchStatuses.Declined
        });
    }


    /// Henter én match fra databasen.
    /// Returnerer null dersom angitt MatchId ikke finnes.
 
    public async Task<Match?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Match>(
            "SELECT * FROM `Match` WHERE MatchId = @MatchId;",
            new { MatchId = id });
    }


    /// Undersøker om kombinasjonen av behov og ressurs allerede er registrert,
    /// uavhengig av matchens status. Brukes før opprettelse av et nytt forslag.
 
    public async Task<bool> ExistsAsync(int needId, int resourceId)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Spørringen returnerer sann dersom minst én match har kombinasjonen.
        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT COUNT(*) > 0
            FROM `Match`
            WHERE NeedId = @NeedId
              AND ResourceId = @ResourceId;
            """,
            new
            {
                NeedId = needId,
                ResourceId = resourceId
            });
    }


    /// Lagrer en ny match og returnerer den genererte match-ID-en.
    /// Controlleren angir behov, ressurs, status, tidspunkt og merknader.
 
    public async Task<int> CreateAsync(Match match)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO `Match` (NeedId, ResourceId, Status, MatchedAt, Notes)
            VALUES (@NeedId, @ResourceId, @Status, @MatchedAt, @Notes);
            SELECT LAST_INSERT_ID();
            """;

        // Dapper henter parameterverdiene fra match-objektet.
        // LAST_INSERT_ID() henter ID-en fra innsettingen på samme tilkobling.
        return await connection.ExecuteScalarAsync<int>(sql, match);
    }


    /// Oppdaterer matchens status og avslutningstidspunkt.
    /// Controlleren kontrollerer statusovergangen og sender inn tidspunktet,
    /// eller null dersom matchen ikke er avsluttet.
 
    public async Task UpdateStatusAsync(
        int id,
        string status,
        DateTime? resolvedAt)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "UPDATE `Match` SET Status = @Status, ResolvedAt = @ResolvedAt WHERE MatchId = @MatchId;",
            new
            {
                MatchId = id,
                Status = status,
                ResolvedAt = resolvedAt
            });
    }
}