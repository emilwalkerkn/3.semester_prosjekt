using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public class ResourceRepository : IResourceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ResourceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Resource>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                           SELECT Id, Type, Description, Location, Latitude, Longitude, GeometryType, GeometryData
                           FROM Resources;
                           """;

        return await connection.QueryAsync<Resource>(sql);
    }

    public async Task<Resource?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                           SELECT Id, Type, Description, Location, Latitude, Longitude
                           FROM Resources
                           WHERE Id = @Id;
                           """;

        return await connection.QuerySingleOrDefaultAsync<Resource>(
            sql,
            new { Id = id }
        );
    }

    public async Task<int> CreateAsync(Resource resource)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
                           INSERT INTO Resources
                               (Type, Description, Location, Latitude, Longitude, GeometryType, GeometryData)
                           VALUES
                               (@Type, @Description, @Location, @Latitude, @Longitude, @GeometryType, @GeometryData);
                           """;

        return await connection.ExecuteAsync(sql, resource);
    }
}