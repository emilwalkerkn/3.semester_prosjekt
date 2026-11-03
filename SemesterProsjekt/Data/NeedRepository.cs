using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public class NeedRepository : INeedRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NeedRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Need>> GetAllAsync(string sortBy = "newest")
    {
        using var connection = _connectionFactory.CreateConnection();

        var orderBy = sortBy switch
        {
            "deadline" => "n.Deadline IS NULL, n.Deadline ASC",
            "priority" => "FIELD(n.Priority, 'Critical', 'High', 'Normal', 'Low')",
            "status" => "n.Status ASC, n.NeedId DESC",
            _ => "n.NeedId DESC"
        };

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

        return await connection.QuerySingleOrDefaultAsync<Need>(
            sql,
            new { NeedId = id }
        );
    }

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

        return await connection.ExecuteScalarAsync<int>(sql, need);
    }

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

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM Needs WHERE NeedId = @NeedId;",
            new { NeedId = id });
    }
}
