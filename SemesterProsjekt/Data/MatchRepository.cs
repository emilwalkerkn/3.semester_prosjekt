using Dapper;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Data;

public class MatchRepository : IMatchRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MatchRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IEnumerable<MatchListItemViewModel>> GetActiveAsync() =>
        GetByHistoryAsync(isClosed: false);

    public Task<IEnumerable<MatchListItemViewModel>> GetHistoryAsync() =>
        GetByHistoryAsync(isClosed: true);

    private async Task<IEnumerable<MatchListItemViewModel>> GetByHistoryAsync(bool isClosed)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<MatchListItemViewModel>(sql, new
        {
            IsClosed = isClosed,
            CompletedStatus = MatchStatuses.Completed,
            DeclinedStatus = MatchStatuses.Declined
        });
    }

    public async Task<Match?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Match>(
            "SELECT * FROM `Match` WHERE MatchId = @MatchId;",
            new { MatchId = id });
    }

    public async Task<bool> ExistsAsync(int needId, int resourceId)
    {
        using var connection = _connectionFactory.CreateConnection();
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

    public async Task<int> CreateAsync(Match match)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO `Match` (NeedId, ResourceId, Status, MatchedAt, Notes)
            VALUES (@NeedId, @ResourceId, @Status, @MatchedAt, @Notes);
            SELECT LAST_INSERT_ID();
            """;
        return await connection.ExecuteScalarAsync<int>(sql, match);
    }

    public async Task UpdateStatusAsync(int id, string status, DateTime? resolvedAt)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE `Match` SET Status = @Status, ResolvedAt = @ResolvedAt WHERE MatchId = @MatchId;",
            new { MatchId = id, Status = status, ResolvedAt = resolvedAt });
    }
}
