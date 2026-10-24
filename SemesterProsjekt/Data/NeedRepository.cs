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

    public async Task<IEnumerable<Need>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                NeedId,
                UserId,
                NeedTypeId,
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
            FROM Needs
            -- Arbeidsrekkefølge i oversikten: behandling først, nye behov
            -- deretter, mens løste behov flyttes helt nederst.
            ORDER BY CASE Status
                WHEN 'In progress' THEN 0
                WHEN 'New' THEN 1
                WHEN 'Resolved' THEN 3
                ELSE 2
            END,
            NeedId DESC;
            """;

        return await connection.QueryAsync<Need>(sql);
    }

    public async Task<Need?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                NeedId,
                UserId,
                NeedTypeId,
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
            FROM Needs
            WHERE NeedId = @NeedId;
            """;

        return await connection.QuerySingleOrDefaultAsync<Need>(
            sql,
            new { NeedId = id }
        );
    }

    public async Task<IEnumerable<NeedType>> GetNeedTypesAsync()
    {
        const string sql = """
                           SELECT
                           NeedTypeId,
                           name AS Name
                           FROM NeedType
                           ORDER BY name;
                           """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<NeedType>(sql);

    }

    public async Task<int> CreateAsync(Need need)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO Needs (
                UserId,
                NeedTypeId,
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
                @UserId,
                @NeedTypeId,
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

    public async Task<bool> UpdateStatusAsync(int id, int userId, bool canManageAll, string status)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Needs
            SET Status = @Status
            WHERE NeedId = @NeedId AND (@CanManageAll = 1 OR UserId = @UserId);
            """;

        return await connection.ExecuteAsync(
            sql,
            new
            {
                NeedId = id,
                UserId = userId,
                CanManageAll = canManageAll,
                Status = status
            }) == 1;
    }
}
