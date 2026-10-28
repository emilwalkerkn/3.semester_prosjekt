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
            SELECT
                ResourceId,
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
            FROM Resources;
            """;

        return await connection.QueryAsync<Resource>(sql);
    }

    public async Task<Resource?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                ResourceId,
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
            FROM Resources
            WHERE ResourceId = @ResourceId;
            """;

        return await connection.QuerySingleOrDefaultAsync<Resource>(
            sql,
            new { ResourceId = id }
        );
    }

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

        return await connection.ExecuteScalarAsync<int>(sql, resource);
    }
}