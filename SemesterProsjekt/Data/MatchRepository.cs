using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public class MatchRepository : IMatchRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MatchRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ResourceMatch>> GetVisibleAsync(int userId, bool canManageAll)
    {
        const string sql = """
            SELECT m.MatchId, m.NeedId, m.ResourceId, n.NeedTypeId, r.ResourceTypeId, m.AssignedQuantity, m.Status, m.CreatedAt,
                   n.Title AS NeedTitle, r.Title AS ResourceTitle, n.Unit,
                   n.UserId AS NeedOwnerId, r.UserId AS ResourceOwnerId
            FROM `Match` m
            INNER JOIN Needs n ON n.NeedId = m.NeedId
            INNER JOIN Resources r ON r.ResourceId = m.ResourceId
            WHERE (@CanManageAll = 1 OR n.UserId = @UserId OR r.UserId = @UserId)
              -- Matchene er arbeidslisten. Avslåtte og ferdig løste behov
              -- beholdes i databasen, men skal ikke vises som aktive matcher.
              AND n.Status <> 'Resolved'
              AND m.Status IN ('Proposed', 'Accepted')
            ORDER BY CASE m.Status WHEN 'Proposed' THEN 0 ELSE 1 END, m.CreatedAt DESC;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ResourceMatch>(sql, new { UserId = userId, CanManageAll = canManageAll });
    }

    public async Task<IEnumerable<Resource>> GetAvailableResourcesAsync()
    {
        const string sql = """
            SELECT ResourceId, UserId, ResourceTypeId, Title, Description, GeometryType, GeometryData,
                   ContactPoint, AvailableFrom, AvailableTo,
                   Quantity - COALESCE(
                       (SELECT SUM(m.AssignedQuantity) FROM `Match` m
                        WHERE m.ResourceId = Resources.ResourceId
                          AND m.Status IN ('Proposed', 'Accepted')), 0) AS Quantity,
                   Unit, Status
            FROM Resources
            WHERE Status = 'Available'
              AND Quantity > COALESCE(
                  (SELECT SUM(m.AssignedQuantity) FROM `Match` m
                   WHERE m.ResourceId = Resources.ResourceId
                     AND m.Status IN ('Proposed', 'Accepted')), 0)
            ORDER BY Title;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Resource>(sql);
    }

    public async Task<int?> CreateAsync(int needId, int resourceId, decimal assignedQuantity, int userId, bool canManageAll)
    {
        // INSERT IGNORE hindrer duplikater. SUM-sjekkene hindrer overbooking.
        const string sql = """
            INSERT IGNORE INTO `Match` (NeedId, ResourceId, AssignedQuantity, Status)
            SELECT n.NeedId, r.ResourceId, @AssignedQuantity, 'Proposed'
            FROM Needs n
            CROSS JOIN Resources r
            WHERE n.NeedId = @NeedId
              AND r.ResourceId = @ResourceId
              AND (@CanManageAll = 1 OR n.UserId = @UserId)
              AND n.Status <> 'Resolved'
              AND r.Status = 'Available'
              AND @AssignedQuantity > 0
              AND @AssignedQuantity <= n.Quantity - COALESCE(
                    (SELECT SUM(m1.AssignedQuantity) FROM `Match` m1
                     WHERE m1.NeedId = n.NeedId AND m1.Status IN ('Proposed', 'Accepted')), 0)
              AND @AssignedQuantity <= r.Quantity - COALESCE(
                    (SELECT SUM(m2.AssignedQuantity) FROM `Match` m2
                     WHERE m2.ResourceId = r.ResourceId AND m2.Status IN ('Proposed', 'Accepted')), 0);
            SELECT LAST_INSERT_ID();
            """;

        using var connection = _connectionFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync<long>(sql, new
        {
            NeedId = needId,
            ResourceId = resourceId,
            AssignedQuantity = assignedQuantity,
            UserId = userId,
            CanManageAll = canManageAll
        });

        return id == 0 ? null : (int)id;
    }

    public async Task<bool> UpdateStatusAsync(int matchId, int userId, bool canManageAll, string status)
    {
        const string sql = """
            UPDATE `Match` m
            INNER JOIN Resources r ON r.ResourceId = m.ResourceId
            SET m.Status = @Status
            WHERE m.MatchId = @MatchId
              AND m.Status = 'Proposed'
              AND @Status IN ('Accepted', 'Rejected')
              AND (@CanManageAll = 1 OR r.UserId = @UserId);
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            UserId = userId,
            CanManageAll = canManageAll,
            Status = status
        }) == 1;
    }
}
