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

    public async Task<bool> UpdateAsync(Resource resource, int userId, bool canManageAll)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Resources
            SET ResourceTypeId = @ResourceTypeId,
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
            WHERE ResourceId = @ResourceId AND (@CanManageAll = 1 OR UserId = @UserId);
            """;

        return await connection.ExecuteAsync(sql, new
        {
            resource.ResourceId,
            resource.ResourceTypeId,
            resource.Title,
            resource.Description,
            resource.GeometryType,
            resource.GeometryData,
            resource.ContactPoint,
            resource.AvailableFrom,
            resource.AvailableTo,
            resource.Quantity,
            resource.Unit,
            resource.Status,
            UserId = userId,
            CanManageAll = canManageAll
        }) == 1;
    }

    public async Task<bool> DeleteAsync(int resourceId, int userId, bool canManageAll)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(
            "DELETE FROM Resources WHERE ResourceId = @ResourceId AND (@CanManageAll = 1 OR UserId = @UserId);",
            new { ResourceId = resourceId, UserId = userId, CanManageAll = canManageAll }) == 1;
    }
}
