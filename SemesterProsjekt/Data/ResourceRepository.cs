using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Håndterer databasetilgang for ressurser og ressurstyper med Dapper.
/// Dapper kobler resultatene fra SQL-spørringene til modellobjekter.
/// Tilgangskontroll og skjemavalidering håndteres i controlleren.

public class ResourceRepository : IResourceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

   
    /// Mottar tilkoblingsfabrikken gjennom dependency injection.
    /// Fabrikken oppretter en egen databasetilkobling for hver metode.
 
    public ResourceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

   
    /// Henter alle ressurser med tilhørende ressurstypenavn.
    /// Resultatet brukes blant annet i oversikten og utskriftsvisningen.
 
    public async Task<IEnumerable<Resource>> GetAllAsync()
    {
        // using sørger for at tilkoblingen frigjøres når metoden avsluttes.
        using var connection = _connectionFactory.CreateConnection();

        // LEFT JOIN beholder ressursen i resultatet selv om typen mangler.
        // Aliaset ResourceTypeName samsvarer med egenskapen i Resource-modellen.
        const string sql = """
            SELECT
                r.ResourceId,
                r.UserId,
                r.ResourceTypeId,
                rt.Name AS ResourceTypeName,
                r.Title,
                r.Description,
                r.GeometryType,
                r.GeometryData,
                r.ContactPoint,
                r.AvailableFrom,
                r.AvailableTo,
                r.Quantity,
                r.Unit,
                r.Status
            FROM Resources r
            LEFT JOIN ResourceType rt ON rt.ResourceTypeId = r.ResourceTypeId;
            """;

        return await connection.QueryAsync<Resource>(sql);
    }

   
    /// Henter én ressurs med tilhørende ressurstypenavn.
    /// Returnerer null dersom ressursen ikke finnes.
 
    public async Task<Resource?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                r.ResourceId,
                r.UserId,
                r.ResourceTypeId,
                rt.Name AS ResourceTypeName,
                r.Title,
                r.Description,
                r.GeometryType,
                r.GeometryData,
                r.ContactPoint,
                r.AvailableFrom,
                r.AvailableTo,
                r.Quantity,
                r.Unit,
                r.Status
            FROM Resources r
            LEFT JOIN ResourceType rt ON rt.ResourceTypeId = r.ResourceTypeId
            WHERE r.ResourceId = @ResourceId;
            """;

        // ID-en sendes som parameter fremfor å settes inn i SQL-teksten.
        // Dette skiller data fra SQL-kode og beskytter mot SQL-injeksjon her.
        return await connection.QuerySingleOrDefaultAsync<Resource>(
            sql,
            new { ResourceId = id }
        );
    }

   
    /// Lagrer en ny ressurs og returnerer den genererte ressurs-ID-en.
    /// UserId skal være satt av controlleren fra den innloggede brukeren.
 
    public async Task<int> CreateAsync(Resource resource)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO Resources (
                UserId,
                ResourceTypeId,
                Title,
                Description,
                GeometryType,
                GeometryData,
                ContactPoint,
                AvailableFrom,
                AvailableTo,
                Quantity,
                Unit,
                Status
            )
            VALUES (
                @UserId,
                @ResourceTypeId,
                @Title,
                @Description,
                @GeometryType,
                @GeometryData,
                @ContactPoint,
                @AvailableFrom,
                @AvailableTo,
                @Quantity,
                @Unit,
                @Status
            );

            SELECT LAST_INSERT_ID();
            """;

        // Dapper henter parameterverdiene fra resource-objektet.
        // LAST_INSERT_ID() henter ID-en fra innsettingen på samme tilkobling.
        return await connection.ExecuteScalarAsync<int>(sql, resource);
    }

   
    /// Henter ressurstypene alfabetisk etter navn.
    /// Typene brukes blant annet som valg i registrerings- og redigeringsskjemaene.
 
    public async Task<IEnumerable<ResourceType>> GetResourceTypesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<ResourceType>(
            "SELECT ResourceTypeId, Name FROM ResourceType ORDER BY Name;");
    }

   
    /// Oppdaterer feltene på ressursen med angitt ResourceId.
    /// Ressursens eier endres ikke. Controlleren må kontrollere
    /// brukerens tilgang før denne metoden kalles.
 
    public async Task UpdateAsync(Resource resource)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Resources SET
                ResourceTypeId = @ResourceTypeId,
                Title = @Title,
                Description = @Description,
                GeometryType = @GeometryType,
                GeometryData = @GeometryData,
                ContactPoint = @ContactPoint,
                AvailableFrom = @AvailableFrom,
                AvailableTo = @AvailableTo,
                Quantity = @Quantity,
                Unit = @Unit,
                Status = @Status
            WHERE ResourceId = @ResourceId;
            """;

        await connection.ExecuteAsync(sql, resource);
    }

   
    /// Sletter ressursen med angitt ID.
    /// Controlleren må kontrollere brukerens tilgang før metoden kalles.
 
    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM Resources WHERE ResourceId = @ResourceId;",
            new { ResourceId = id });
    }
}