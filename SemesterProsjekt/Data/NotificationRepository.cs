using Dapper;
using SemesterProsjekt.Models;


namespace SemesterProsjekt.Data;

/// Dapper-implementasjon av varsler. Alle spørringer filtrerer på UserId,
/// slik at en bruker aldri kan lese eller endre andres varsler.
public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(string userId, string message, string? link)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO Notifications (UserId, Message, Link, IsRead, CreatedAt)
            VALUES (@UserId, @Message, @Link, FALSE, @CreatedAt);
            """,
            new { UserId = userId, Message = message, Link = link, CreatedAt = DateTime.UtcNow });
    }

    public async Task<IEnumerable<Notification>> GetLatestForUserAsync(string userId, int count)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Notification>("""
            SELECT NotificationId, UserId, Message, Link, IsRead, CreatedAt
            FROM Notifications
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC, NotificationId DESC
            LIMIT @Count;
            """,
            new { UserId = userId, Count = count });
    }

    public async Task<int> CountUnreadAsync(string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Notifications WHERE UserId = @UserId AND IsRead = FALSE;",
            new {UserId = userId });
    }

    public async Task<string?> MarkAsReadAsync(int notificationId, string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new { Id = notificationId, UserId = userId };

        await connection.ExecuteAsync(
            "UPDATE Notifications SET IsRead = TRUE WHERE NotificationId = @Id AND UserId = @UserId;",
            parameters);

        return await connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT Link FROM Notifications WHERE NotificationId = @Id AND UserId = @UserId;",
            parameters);
   }

   public async Task MarkAllAsReadAsync(string UserId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE Notifications SET IsRead = TRUE WHERE UserId = @UserId AND IsRead = FALSE;",
            new { UserId = UserId });
    }

}