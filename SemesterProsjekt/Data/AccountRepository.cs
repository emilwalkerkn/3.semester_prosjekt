using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AccountRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ProfileSettingsViewModel?> GetProfileAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                appUser.FirstName,
                appUser.LastName,
                appUser.Email,
                COALESCE(GROUP_CONCAT(role.Name SEPARATOR ', '), 'Ingen rolle') AS Role,
                appUser.Username
            FROM Users appUser
            LEFT JOIN UserRole userRole ON userRole.UserId = appUser.UserId
            LEFT JOIN Roles role ON role.RoleId = userRole.RoleId
            WHERE appUser.UserId = @UserId
            GROUP BY appUser.UserId, appUser.FirstName, appUser.LastName, appUser.Email, appUser.Username;
            """;

        return await connection.QuerySingleOrDefaultAsync<ProfileSettingsViewModel>(sql, new { UserId = userId });
    }

    public async Task UpdateProfileAsync(int userId, ProfileSettingsViewModel profile)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Users
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email
            WHERE UserId = @UserId;
            """;

        await connection.ExecuteAsync(sql, new { UserId = userId, profile.FirstName, profile.LastName, profile.Email });
    }

    public async Task<string?> GetPasswordHashAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT PasswordHash FROM Users WHERE UserId = @UserId;",
            new { UserId = userId });
    }

    public async Task UpdatePasswordHashAsync(int userId, string passwordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId;",
            new { UserId = userId, PasswordHash = passwordHash });
    }

    public async Task<IEnumerable<string>> GetRoleNamesAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>("""
            SELECT role.Name
            FROM Roles role
            INNER JOIN UserRole userRole ON userRole.RoleId = role.RoleId
            WHERE userRole.UserId = @UserId;
            """, new { UserId = userId });
    }
}
