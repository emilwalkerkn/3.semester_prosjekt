using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Håndterer databasetilgang for behov og behovstyper med Dapper.
/// Dapper kobler resultatene fra SQL-spørringene til modellobjekter.
/// Tilgangskontroll og skjemavalidering håndteres i controlleren.

public class NeedRepository : INeedRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

  
    /// Mottar tilkoblingsfabrikken gjennom dependency injection.
    /// Fabrikken oppretter en egen databasetilkobling for hver metode.
  
    public NeedRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

  
    /// Henter alle behov med tilhørende behovstypenavn og valgt sortering.
    /// Standardvalget viser behovene med høyest ID først.
  
    public async Task<IEnumerable<Need>> GetAllAsync(string sortBy = "newest")
    {
        // using sørger for at tilkoblingen frigjøres når metoden avsluttes.
        using var connection = _connectionFactory.CreateConnection();

        // Sorteringen velges fra faste SQL-uttrykk.
        // Brukerens tekst settes ikke direkte inn i SQL-spørringen.
        // Ukjente valg bruker standardsorteringen.
        var orderBy = sortBy switch
        {
            // Tidligste frist først. Behov uten frist plasseres sist.
            "deadline" => "n.Deadline IS NULL, n.Deadline ASC",

            // FIELD angir en bestemt rekkefølge for de kjente prioritetene.
            "priority" => "FIELD(n.Priority, 'Critical', 'High', 'Normal', 'Low')",

            // Sorterer etter statustekst, deretter høyest ID innen samme status.
            "status" => "n.Status ASC, n.NeedId DESC",

            _ => "n.NeedId DESC"
        };

        // LEFT JOIN beholder behovet i resultatet selv om typen mangler.
        // Aliaset NeedTypeName samsvarer med egenskapen i Need-modellen.
        var sql = $"""
            SELECT
                n.NeedId,
                n.UserId,
                n.NeedTypeId,
                nt.Name AS NeedTypeName,
                n.Title,
                n.Description,
                n.GeometryType,
                n.GeometryData,
                n.ContactPoint,
                n.NeededFrom,
                n.Deadline,
                n.Quantity,
                n.Unit,
                n.Status,
                n.Priority
            FROM Needs n
            LEFT JOIN NeedType nt ON nt.NeedTypeId = n.NeedTypeId
            ORDER BY {orderBy};
            """;

        return await connection.QueryAsync<Need>(sql);
    }

  
    /// Henter ett behov med tilhørende behovstypenavn.
    /// Returnerer null dersom behovet ikke finnes.
  
    public async Task<Need?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                n.NeedId,
                n.UserId,
                n.NeedTypeId,
                nt.Name AS NeedTypeName,
                n.Title,
                n.Description,
                n.GeometryType,
                n.GeometryData,
                n.ContactPoint,
                n.NeededFrom,
                n.Deadline,
                n.Quantity,
                n.Unit,
                n.Status,
                n.Priority
            FROM Needs n
            LEFT JOIN NeedType nt ON nt.NeedTypeId = n.NeedTypeId
            WHERE n.NeedId = @NeedId;
            """;

        // ID-en sendes som parameter fremfor å settes inn i SQL-teksten.
        // Dette skiller data fra SQL-kode og beskytter mot SQL-injeksjon her.
        return await connection.QuerySingleOrDefaultAsync<Need>(
            sql,
            new { NeedId = id }
        );
    }

  
    /// Henter behovstypene alfabetisk etter navn.
    /// Typene brukes blant annet som valg i registrerings- og redigeringsskjemaene.
  
    public async Task<IEnumerable<NeedType>> GetNeedTypesAsync()
    {
        const string sql = """
            SELECT NeedTypeId, Name
            FROM NeedType
            ORDER BY Name;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<NeedType>(sql);
    }

  
    /// Lagrer et nytt behov og returnerer den genererte behovs-ID-en.
    /// UserId skal være satt av controlleren fra den innloggede brukeren.
  
    public async Task<int> CreateAsync(Need need)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO Needs (
                NeedTypeId,
                UserId,
                Title,
                Description,
                GeometryType,
                GeometryData,
                ContactPoint,
                NeededFrom,
                Deadline,
                Quantity,
                Unit,
                Status,
                Priority
            )
            VALUES (
                @NeedTypeId,
                @UserId,
                @Title,
                @Description,
                @GeometryType,
                @GeometryData,
                @ContactPoint,
                @NeededFrom,
                @Deadline,
                @Quantity,
                @Unit,
                @Status,
                @Priority
            );

            SELECT LAST_INSERT_ID();
            """;

        // Dapper henter parameterverdiene fra need-objektet.
        // LAST_INSERT_ID() henter ID-en fra innsettingen på samme tilkobling.
        return await connection.ExecuteScalarAsync<int>(sql, need);
    }

  
    /// Oppdaterer bare statusen til behovet med angitt ID.
    /// Controlleren må kontrollere både tillatt status og brukerens tilgang.
  
    public async Task UpdateStatusAsync(int id, string status)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Needs
            SET Status = @Status
            WHERE NeedId = @NeedId;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                NeedId = id,
                Status = status
            }
        );
    }

  
    /// Oppdaterer feltene på behovet med angitt NeedId.
    /// Behovets eier endres ikke. Controlleren må kontrollere
    /// brukerens tilgang før denne metoden kalles.
  
    public async Task UpdateAsync(Need need)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Needs SET
                NeedTypeId = @NeedTypeId,
                Title = @Title,
                Description = @Description,
                GeometryType = @GeometryType,
                GeometryData = @GeometryData,
                ContactPoint = @ContactPoint,
                NeededFrom = @NeededFrom,
                Deadline = @Deadline,
                Quantity = @Quantity,
                Unit = @Unit,
                Status = @Status,
                Priority = @Priority
            WHERE NeedId = @NeedId;
            """;

        await connection.ExecuteAsync(sql, need);
    }

  
    /// Sletter behovet med angitt ID.
    /// Controlleren må kontrollere brukerens tilgang før metoden kalles.
  
    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM Needs WHERE NeedId = @NeedId;",
            new { NeedId = id });
    }
}